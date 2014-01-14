using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.DirectX.Direct3D;
using System.Threading;
using Microsoft.DirectX;
using Utility;

namespace AppScene
{
    public partial class SceneControl : UserControl
    {
        private Device m_Device3d;
        private PresentParameters m_presentParams;
        private DrawArgs drawArgs;
        private World m_World;

        public World CurrentWorld
        {
            get { return m_World; }
            set { m_World = value; }
        }
        private Thread m_WorkerThread;
        private bool m_WorkerThreadRunning;
        private System.Timers.Timer m_FpsTimer = new System.Timers.Timer(250);
        //是否移动鼠标
        private bool isMouseDragging;
        public SceneControl()
        {
            InitializeComponent();
            InitializeGraphics();
            drawArgs = new DrawArgs(m_Device3d, this);
            m_World = new World("世界");
            this.drawArgs.WorldCamera = new Camera();
        }
        private void InitializeGraphics()
        {
            m_presentParams = new PresentParameters();
            m_presentParams.Windowed = true;
            m_presentParams.SwapEffect = SwapEffect.Discard;
            m_presentParams.AutoDepthStencilFormat = DepthFormat.D16;
            m_presentParams.EnableAutoDepthStencil = true;
            m_presentParams.PresentationInterval = PresentInterval.Immediate;
            int adapterOrdinal = 0;
            try
            {
                adapterOrdinal = Manager.Adapters.Default.Adapter;
            }
            catch
            {
                // User probably needs to upgrade DirectX or install a 3D capable graphics adapter
                throw new NotAvailableException();
            }

            DeviceType dType = DeviceType.Hardware;
            foreach (AdapterInformation ai in Manager.Adapters)
            {
                if (ai.Information.Description.IndexOf("NVPerfHUD") >= 0)
                {
                    adapterOrdinal = ai.Adapter;
                    dType = DeviceType.Reference;
                }
            }
            CreateFlags flags = CreateFlags.SoftwareVertexProcessing;

            // Check to see if we can use a pure hardware m_Device3d
            Caps caps = Manager.GetDeviceCaps(adapterOrdinal, DeviceType.Hardware);

            // Do we support hardware vertex processing?
            if (caps.DeviceCaps.SupportsHardwareTransformAndLight)
                //	// Replace the software vertex processing
                flags = CreateFlags.HardwareVertexProcessing;

            // Use multi-threading for now - TODO: See if the code can be changed such that this isn't necessary (Texture Loading for example)
            flags |= CreateFlags.MultiThreaded | CreateFlags.FpuPreserve;
            // flags = CreateFlags.SoftwareVertexProcessing;
            try
            {
                //实例化设备对象
                m_Device3d = new Device(adapterOrdinal, dType, this, flags, m_presentParams);
            }
            catch (Microsoft.DirectX.DirectXException)
            {
                throw new NotSupportedException("Unable to create the Direct3D m_Device3d.");
            }

            //绑定事件
            m_Device3d.DeviceReset += new EventHandler(OnDeviceReset);
            m_Device3d.DeviceResizing += new CancelEventHandler(m_Device3d_DeviceResizing);
            OnDeviceReset(m_Device3d, null);
        }

        private void OnDeviceReset(object sender, EventArgs e)
        {
            // Can we use anisotropic texture minify filter?
            if (m_Device3d.DeviceCaps.TextureFilterCaps.SupportsMinifyAnisotropic)
            {
                m_Device3d.SamplerState[0].MinFilter = TextureFilter.Anisotropic;
            }
            else if (m_Device3d.DeviceCaps.TextureFilterCaps.SupportsMinifyLinear)
            {
                m_Device3d.SamplerState[0].MinFilter = TextureFilter.Linear;
            }

            // What about magnify filter?
            if (m_Device3d.DeviceCaps.TextureFilterCaps.SupportsMagnifyAnisotropic)
            {
                m_Device3d.SamplerState[0].MagFilter = TextureFilter.Anisotropic;
            }
            else if (m_Device3d.DeviceCaps.TextureFilterCaps.SupportsMagnifyLinear)
            {
                m_Device3d.SamplerState[0].MagFilter = TextureFilter.Linear;
            }

            m_Device3d.SamplerState[0].AddressU = TextureAddress.Clamp;
            m_Device3d.SamplerState[0].AddressV = TextureAddress.Clamp;

            m_Device3d.RenderState.Clipping = true;
            //Clockwise不显示按顺时针绘制的三角形
            m_Device3d.RenderState.CullMode = Cull.None;
            m_Device3d.RenderState.Lighting = false;
            // m_Device3d.RenderState.Ambient = World.Settings.StandardAmbientColor;

            m_Device3d.RenderState.ZBufferEnable = true;
            m_Device3d.RenderState.AlphaBlendEnable = true;
            m_Device3d.RenderState.SourceBlend = Blend.SourceAlpha;
            m_Device3d.RenderState.DestinationBlend = Blend.InvSourceAlpha;
        }
        private void m_Device3d_DeviceResizing(object sender, CancelEventArgs e)
        {
            if (this.Size.Width == 0 || this.Size.Height == 0)
            {
                e.Cancel = true;
                return;
            }

            this.drawArgs.screenHeight = this.Height;
            this.drawArgs.screenWidth = this.Width;
        }
        protected void AttemptRecovery()
        {
            try
            {
                m_Device3d.TestCooperativeLevel();
            }
            catch (DeviceLostException)
            {
            }
            catch (DeviceNotResetException)
            {
                try
                {
                    m_Device3d.Reset(m_presentParams);
                }
                catch (DeviceLostException)
                {
                    // If it's still lost or lost again, just do
                    // nothing
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Paint the last active scene if rendering is disabled to keep the ui responsive
            try
            {
                if (m_Device3d == null)
                {
                    e.Graphics.Clear(SystemColors.Control);
                    return;
                }

                // to prevent screen garbage when resizing
                Render();
                m_Device3d.Present();
            }
            catch (DeviceLostException)
            {
                try
                {
                    AttemptRecovery();
                    // Our surface was lost, force re-render
                    Render();
                    m_Device3d.Present();
                }
                catch (DirectXException)
                {
                    // Ignore a 2nd failure
                }
            }
        }
        /// <summary>
        /// Background worker thread loop (updates UI)
        /// </summary>
        private void WorkerThreadFunc()
        {
            const int refreshIntervalMs = 150; // Max 6 updates per seconds
            while (m_WorkerThreadRunning)
            {
                try
                {
                    //if (World.Settings.UseBelowNormalPriorityUpdateThread && m_WorkerThread.Priority == System.Threading.ThreadPriority.Normal)
                    //{
                    //    m_WorkerThread.Priority = System.Threading.ThreadPriority.BelowNormal;
                    //}
                    //else if (!World.Settings.UseBelowNormalPriorityUpdateThread && m_WorkerThread.Priority == System.Threading.ThreadPriority.BelowNormal)
                    //{
                    //    m_WorkerThread.Priority = System.Threading.ThreadPriority.Normal;
                    //}

                    m_World.Update(this.drawArgs);
                }
                catch (Exception caught)
                {
                    Log.Write(caught);
                }
            }
        }
        /// <summary>
        /// Determine whether any window messages is queued.
        /// </summary>
        private static bool IsAppStillIdle
        {
            get
            {
                NativeMethods.Message msg;
                return !NativeMethods.PeekMessage(out msg, IntPtr.Zero, 0, 0, 0);
            }
        }
        public void OnApplicationIdle(object sender, EventArgs e)
        {
            if (Parent.Focused && !Focused)
                Focus();
            while (IsAppStillIdle)
            {
                Render();
                drawArgs.Present();
            }
            //Application.DoEvents();
        }
        private double mapWidth = 0;
        // 建立相机
        private void CameraViewSetup()
        {
            float aspectRatio = (float)m_Device3d.Viewport.Width / m_Device3d.Viewport.Height;
            m_Device3d.Transform.Projection = Matrix.PerspectiveFovLH(fov, aspectRatio, mapWidth == 0 ? 30.0f : (float)(mapWidth / 10), mapWidth == 0 ? 1000f : (float)(mapWidth * 3));
            // m_Device3d.Transform.View = Matrix.LookAtLH(new Vector3(0, 0, dist), new Vector3(0, 0, 0), new Vector3(1, 0, 0));
        }
        public void Render()
        {
            try
            {
                this.drawArgs.BeginRender();

                // Render the sky according to view - example, close to earth, render sky blue, render space as black
                System.Drawing.Color backgroundColor = System.Drawing.Color.DarkSlateBlue;
                m_Device3d.Clear(ClearFlags.Target | ClearFlags.ZBuffer, backgroundColor, 1.0f, 0);

                if (m_World == null)
                {
                    m_Device3d.BeginScene();
                    m_Device3d.EndScene();
                    m_Device3d.Present();
                    Thread.Sleep(25);
                    return;
                }

                if (m_WorkerThread == null)
                {
                    m_WorkerThreadRunning = true;
                    m_WorkerThread = new Thread(new ThreadStart(WorkerThreadFunc));
                    m_WorkerThread.Name = "WorldWindow.WorkerThreadFunc";
                    m_WorkerThread.IsBackground = true;
                    //if (World.Settings.UseBelowNormalPriorityUpdateThread)
                    //{
                    //    m_WorkerThread.Priority = ThreadPriority.BelowNormal;
                    //}
                    //else
                    //{
                    //    m_WorkerThread.Priority = ThreadPriority.Normal;
                    //}
                    // BelowNormal makes rendering smooth, but on slower machines updates become slow or stops
                    // TODO: Implement dynamic FPS limiter (or different solution)
                    m_WorkerThread.Start();
                }


                // Rendering here
                CameraViewSetup();
                this.drawArgs.WorldCamera.Update(m_Device3d);
                // Translation and Orientation angle / angle2
                if (angle2 < 0) angle2 = 0;
                if (angle2 > Math.PI / 2) angle2 = (float)(Math.PI / 2);
                m_Device3d.Transform.World = Matrix.Translation(dy, dx, dz);
                //m_Device3d.Transform.World *= Matrix.RotationY(angle);
                //m_Device3d.Transform.World *= Matrix.RotationX(angle2);


                m_Device3d.BeginScene();

                //设置绘制模式，是线框模式？
                if (drawArgs.RenderWireFrame)
                    m_Device3d.RenderState.FillMode = FillMode.WireFrame;
                else
                    m_Device3d.RenderState.FillMode = FillMode.Solid;
                // 渲染世界对象
                m_World.Render(this.drawArgs);

                drawArgs.Device.RenderState.ZBufferEnable = false;

                //设置启用雾效
                if (drawArgs.FogEnable == false)
                    m_Device3d.RenderState.FogEnable = false;

                m_Device3d.EndScene();
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
            finally
            {
                this.drawArgs.EndRender();
            }
            drawArgs.UpdateMouseCursor(this);
        }


        /// <summary>
        ///清除对象
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (disposing && (components != null))
                {
                    components.Dispose();
                }
                if (m_WorkerThread != null && m_WorkerThread.IsAlive)
                {
                    m_WorkerThreadRunning = false;
                    m_WorkerThread.Abort();
                }

                // m_FpsTimer.Stop();
                if (m_World != null)
                {
                    m_World.Dispose();
                    m_World = null;
                }
                if (this.drawArgs != null)
                {
                    this.drawArgs.Dispose();
                    this.drawArgs = null;
                }


                m_Device3d.Dispose();
                /*
                                if(m_downloadIndicator != null)
                                {
                                    m_downloadIndicator.Dispose();
                                    m_downloadIndicator = null;
                                }
                */
            }

            base.Dispose(disposing);
            GC.SuppressFinalize(this);
        }

        private Point mouseDownStartPosition = Point.Empty;
        private float mouseDownStartDx;
        private float mouseDownStartDy;
        private float mouseDownStartAngle;
        private float mouseDownStartAngle2;
        private float mouseDownLightAngle;
        private float mouseDownLightAngle2;
        private float lightHeading = (float)(-Math.PI / 4);		// Light source direction
        private float lightElevation = (float)(Math.PI / 4);
        private bool redraw = true;						// Redraw scene when true
        private float angle = 0.0f;// (float)Math.PI / 2;		// Spin angle on Z (map rotation)
        private float angle2 = 0.0f;// (float)Math.PI / 4;		// Spin angle on Y (map inclinaison)
        private float dx = 0;							// Map displacement X
        private float dy = 0;							// Map displacement Y
        private float dz = 0;							// Map displacement Z (height)
        private float dist = 150;						// Camera distance from target on map
        float fov = (float)Math.PI / 4;					// Camera field of view - radian
        int rightClickFactor = -1;

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            // Zoom in and out
            //this.dist -= this.dist * (e.Delta / 3000.0f);
            drawArgs.WorldCamera.walk(this.dist * (e.Delta / 3000.0f));
            redraw = true;
        }

        private void SceneControl_MouseDown(object sender, MouseEventArgs e)
        {
            this.Focus();  //fixes mousewheel not working problem

            // Save mouse position and map transform values at that point
            mouseDownStartPosition.X = e.X;
            mouseDownStartPosition.Y = e.Y;
            mouseDownStartDx = dx;
            mouseDownStartDy = dy;
            mouseDownStartAngle = angle;
            mouseDownStartAngle2 = angle2;
            mouseDownLightAngle = lightHeading;
            mouseDownLightAngle2 = lightElevation;
        }
        public bool HandleKeyUp(KeyEventArgs e)
        {
            //Alt键按下
            if (e.Alt)
            {
            }
            //Ctrl键按下
            else if (e.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.W:
                        //切换线框绘制模式
                        drawArgs.RenderWireFrame = !drawArgs.RenderWireFrame;
                        return true;
                }
            }
            //其他键
            else
            {
                switch (e.KeyCode)
                {
                    case Keys.Space:
                    case Keys.Clear:
                        // this.drawArgs.WorldCamera.Reset();
                        return true;
                }
            }
            return false;
        }
        private void SceneControl_MouseMove(object sender, MouseEventArgs e)
        {
            //默认cursor
            DrawArgs.MouseCursor = CursorType.Arrow;
            if (mouseDownStartPosition == Point.Empty)
                return;
            // Mouse drag
            bool isMouseLeftButtonDown = ((int)e.Button & (int)MouseButtons.Left) != 0;
            bool isMouseRightButtonDown = ((int)e.Button & (int)MouseButtons.Right) != 0;
            double dxMouse = this.mouseDownStartPosition.X - e.X;
            double dyMouse = this.mouseDownStartPosition.Y - e.Y;
            if (isMouseLeftButtonDown && !isMouseRightButtonDown)
            {
                // Move map
                isMouseDragging = true;
                double moveFactor = dist * 0.001f;
                dx = mouseDownStartDx - (float)(Math.Sin(angle) * dxMouse * moveFactor) + (float)(Math.Cos(angle) * dyMouse * moveFactor);
                dy = mouseDownStartDy - (float)(Math.Cos(angle) * dxMouse * moveFactor) - (float)(Math.Sin(angle) * dyMouse * moveFactor);
                redraw = true;
            }
            if (isMouseRightButtonDown && !isMouseLeftButtonDown)
            {
                // Rotate map
                double spinFactor = 0.0001f;
                angle = mouseDownStartAngle + (float)(dxMouse * spinFactor * rightClickFactor);
                angle2 = mouseDownStartAngle2 + (float)(dyMouse * spinFactor * rightClickFactor);
                drawArgs.WorldCamera.RotateRay(angle, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f));
               // drawArgs.WorldCamera.RotateRay(angle2, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 0.0f, 0.0f));
                redraw = true;
            }
            if (isMouseRightButtonDown && isMouseLeftButtonDown)
            {
                // Rotate light
                double spinFactor = 0.003f;
                lightHeading = mouseDownLightAngle + (float)(dxMouse * spinFactor);
                lightElevation = mouseDownLightAngle2 + (float)(dyMouse * spinFactor);
                redraw = true;
            }
        }

        private void SceneControl_MouseUp(object sender, MouseEventArgs e)
        {
            DrawArgs.LastMousePosition.X = e.X;
            DrawArgs.LastMousePosition.Y = e.Y;

            mouseDownStartPosition = Point.Empty;
            redraw = true;
            if (e.Button == MouseButtons.Left)
            {
                if (this.isMouseDragging)
                {
                    this.isMouseDragging = false;
                }
                else
                {
                    if (!m_World.PerformSelectionAction(this.drawArgs))
                    {

                        //Angle targetLatitude;
                        //Angle targetLongitude;
                        ////Quaternion targetOrientation = new Quaternion();
                        //this.drawArgs.WorldCamera.PickingRayIntersection(
                        //    DrawArgs.LastMousePosition.X,
                        //    DrawArgs.LastMousePosition.Y,
                        //    out targetLatitude,
                        //    out targetLongitude);
                        //if (!Angle.IsNaN(targetLatitude))
                        //    this.drawArgs.WorldCamera.PointGoto(targetLatitude, targetLongitude);
                    }
                }
            }
        }
        protected override void OnKeyUp(KeyEventArgs e)
        {
            try
            {
                e.Handled = HandleKeyUp(e);
                base.OnKeyUp(e);
            }
            catch (Exception caught)
            {
                MessageBox.Show(caught.Message, "操作失败！", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void SceneControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (m_Device3d != null)
            {
                //
                // 更新相机
                //
                float timeDelta = 0.01f;
                if (e.KeyCode == Keys.W)
                    drawArgs.WorldCamera.walk(-20.0f * timeDelta);

                if (e.KeyCode == Keys.S)
                    drawArgs.WorldCamera.walk(20.0f * timeDelta);

                if (e.KeyCode == Keys.A)
                    drawArgs.WorldCamera.strafe(-10.0f * timeDelta);

                if (e.KeyCode == Keys.D)
                    drawArgs.WorldCamera.strafe(10.0f * timeDelta);

                if (e.KeyCode == Keys.Up)
                    drawArgs.WorldCamera.fly(20.0f * timeDelta);

                if (e.KeyCode == Keys.Down)
                    drawArgs.WorldCamera.fly(-20.0f * timeDelta);

                if (e.KeyCode == Keys.E)
                    drawArgs.WorldCamera.Pitch(1.0f * timeDelta);

                if (e.KeyCode == Keys.F)
                    drawArgs.WorldCamera.Pitch(-1.0f * timeDelta);

                if (e.KeyCode == Keys.J)
                    drawArgs.WorldCamera.Yaw(-1.0f * timeDelta);

                if (e.KeyCode == Keys.I)
                    drawArgs.WorldCamera.Yaw(1.0f * timeDelta);

                if (e.KeyCode == Keys.N)
                    drawArgs.WorldCamera.Roll(1.0f * timeDelta);

                if (e.KeyCode == Keys.M)
                    drawArgs.WorldCamera.Roll(-1.0f * timeDelta);

                // Update the view matrix representing the cameras 
                // new position/orientation.

            }
        }
    }
}
