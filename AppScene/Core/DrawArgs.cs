using System;
using System.Diagnostics;
using System.Collections;
using System.IO;
using System.Net;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace AppScene
{
    /// <summary>
    /// 
    /// </summary>
    public class DrawArgs : IDisposable
    {
        private Device m_device;

        public Device Device
        {
            get { return m_device; }
            set { m_device = value; }
        }
        public System.Windows.Forms.Control parentControl;
        public static System.Windows.Forms.Control ParentControl = null;
        public int numBoundaryPointsTotal;
        public int numBoundaryPointsRendered;
        public int numBoundariesDrawn;

        public System.Drawing.Font defaultSubTitleFont;

        public int screenWidth;
        public int screenHeight;
        public static System.Drawing.Point LastMousePosition;
        public int numberTilesDrawn;
        public System.Drawing.Point CurrentMousePosition;
        public string UpperLeftCornerText = "";
        Camera m_WorldCamera;
        public World m_CurrentWorld = null;
        public static bool IsLeftMouseButtonDown = false;
        public static bool IsRightMouseButtonDown = false;

        public int TexturesLoadedThisFrame = 0;
        private static System.Drawing.Bitmap bitmap;
        public static System.Drawing.Graphics Graphics = null;

        public bool RenderWireFrame = false;

        /// <summary>
        /// Table of all icon textures
        /// </summary>
        protected static Hashtable m_textures = new Hashtable();
        public static Hashtable Textures
        {
            get { return m_textures; }
        }

        public static Camera Camera = null;
        public Camera WorldCamera
        {
            get
            {
                return m_WorldCamera;
            }
            set
            {
                m_WorldCamera = value;
                Camera = value;
            }
        }

        public World CurrentWorld
        {
            get
            {
                return m_CurrentWorld;
            }
            set
            {
                m_CurrentWorld = value;
            }
        }

        /// <summary>
        /// Absolute time of current frame render start (ticks)
        /// </summary>
        public static long CurrentFrameStartTicks;

        /// <summary>
        /// Seconds elapsed between start of previous frame and start of current frame.
        /// </summary>
        public static float LastFrameSecondsElapsed;

        static CursorType mouseCursor;
        static CursorType lastCursor;
        bool repaint = true;
        bool isPainting;
        Hashtable fontList = new Hashtable();

        public static Device sDevice = null;
        System.Windows.Forms.Cursor measureCursor;

        public DrawArgs(Device device, System.Windows.Forms.Control parentForm)
        {
            this.parentControl = parentForm;
            DrawArgs.ParentControl = parentForm;
            DrawArgs.sDevice = device;
            this.m_device = device;


            bitmap = new System.Drawing.Bitmap(256, 256, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            DrawArgs.Graphics = System.Drawing.Graphics.FromImage(bitmap);
            //	InitializeReference();
        }

        System.Windows.Forms.Control m_ReferenceForm;
        private void InitializeReference()
        {
            PresentParameters presentParameters = new PresentParameters();
            presentParameters.Windowed = true;
            presentParameters.SwapEffect = SwapEffect.Discard;
            presentParameters.AutoDepthStencilFormat = DepthFormat.D16;
            presentParameters.EnableAutoDepthStencil = true;

            m_ReferenceForm = new System.Windows.Forms.Control("Reference", 0, 0, 1, 1);
            m_ReferenceForm.Visible = false;

            int adapterOrdinal = 0;
            try
            {
                // Store the default adapter
                adapterOrdinal = Manager.Adapters.Default.Adapter;
            }
            catch
            {
                // User probably needs to upgrade DirectX or install a 3D capable graphics adapter
                throw new NotAvailableException();
            }

            //		DeviceType dType = DeviceType.Reference;

            CreateFlags flags = CreateFlags.SoftwareVertexProcessing;

            flags |= CreateFlags.MultiThreaded | CreateFlags.FpuPreserve;
            /*
                        try
                        {
                            // Create our m_Device3d
                            m_Device3dReference = new Device(adapterOrdinal, dType, m_ReferenceForm, flags, presentParameters);
                        }
                        catch( Microsoft.DirectX.DirectXException	)
                        {
                            throw new NotSupportedException("Unable to create the Direct3D m_Device3d.");
                        }

                        // Hook the m_Device3d reset event
                        m_Device3dReference.DeviceReset += new EventHandler(OnDeviceReset);
                    //	m_Device3dReference.DeviceResizing += new CancelEventHandler(m_Device3d_DeviceResizing);
                        OnDeviceReset(m_Device3dReference, null);
                        */
        }

        private void OnDeviceReset(object sender, EventArgs e)
        {
            // Can we use anisotropic texture minify filter?
            if (m_Device3dReference.DeviceCaps.TextureFilterCaps.SupportsMinifyAnisotropic)
            {
                m_Device3dReference.SamplerState[0].MinFilter = TextureFilter.Anisotropic;
            }
            else if (m_Device3dReference.DeviceCaps.TextureFilterCaps.SupportsMinifyLinear)
            {
                m_Device3dReference.SamplerState[0].MinFilter = TextureFilter.Linear;
            }

            // What about magnify filter?
            if (m_Device3dReference.DeviceCaps.TextureFilterCaps.SupportsMagnifyAnisotropic)
            {
                m_Device3dReference.SamplerState[0].MagFilter = TextureFilter.Anisotropic;
            }
            else if (m_Device3dReference.DeviceCaps.TextureFilterCaps.SupportsMagnifyLinear)
            {
                m_Device3dReference.SamplerState[0].MagFilter = TextureFilter.Linear;
            }

            m_Device3dReference.SamplerState[0].AddressU = TextureAddress.Clamp;
            m_Device3dReference.SamplerState[0].AddressV = TextureAddress.Clamp;

            m_Device3dReference.RenderState.Clipping = true;
            m_Device3dReference.RenderState.CullMode = Cull.Clockwise;
            m_Device3dReference.RenderState.Lighting = false;
            m_Device3dReference.RenderState.Ambient = System.Drawing.Color.FromArgb(0x40, 0x40, 0x40);

            m_Device3dReference.RenderState.ZBufferEnable = true;
            m_Device3dReference.RenderState.AlphaBlendEnable = true;
            m_Device3dReference.RenderState.SourceBlend = Blend.SourceAlpha;
            m_Device3dReference.RenderState.DestinationBlend = Blend.InvSourceAlpha;
        }

        Device m_Device3dReference = null;
        public void BeginRender()
        {
            // Development variable to see the number of tiles drawn - Added for frustum culling testing
            this.numberTilesDrawn = 0;

            this.TexturesLoadedThisFrame = 0;

            this.UpperLeftCornerText = "";
            this.numBoundaryPointsRendered = 0;
            this.numBoundaryPointsTotal = 0;
            this.numBoundariesDrawn = 0;

            this.isPainting = true;
        }

        public void EndRender()
        {
            Debug.Assert(isPainting);
            this.isPainting = false;
        }

        /// <summary>
        /// Displays the rendered image (call after EndRender)
        /// </summary>
        public void Present()
        {
            // Calculate frame time
            long previousFrameStartTicks = CurrentFrameStartTicks;
            //PerformanceTimer.QueryPerformanceCounter(ref CurrentFrameStartTicks);
            //LastFrameSecondsElapsed = (CurrentFrameStartTicks - previousFrameStartTicks) / 
            //    (float)PerformanceTimer.TicksPerSecond;

            // Display the render
            m_device.Present();
        }

        /// <summary>
        /// Active mouse cursor
        /// </summary>
        public static CursorType MouseCursor
        {
            get
            {
                return mouseCursor;
            }
            set
            {
                mouseCursor = value;
            }
        }

        public void UpdateMouseCursor(System.Windows.Forms.Control parent)
        {
            if (lastCursor == mouseCursor)
                return;

            switch (mouseCursor)
            {
                case CursorType.Hand:
                    parent.Cursor = System.Windows.Forms.Cursors.Hand;
                    break;
                case CursorType.Cross:
                    parent.Cursor = System.Windows.Forms.Cursors.Cross;
                    break;
                case CursorType.Measure:
                    if (measureCursor == null)
                        //measureCursor = ImageHelper.LoadCursor("measure.cur");
                        parent.Cursor = measureCursor;
                    break;
                case CursorType.SizeWE:
                    parent.Cursor = System.Windows.Forms.Cursors.SizeWE;
                    break;
                case CursorType.SizeNS:
                    parent.Cursor = System.Windows.Forms.Cursors.SizeNS;
                    break;
                case CursorType.SizeNESW:
                    parent.Cursor = System.Windows.Forms.Cursors.SizeNESW;
                    break;
                case CursorType.SizeNWSE:
                    parent.Cursor = System.Windows.Forms.Cursors.SizeNWSE;
                    break;
                default:
                    parent.Cursor = System.Windows.Forms.Cursors.Arrow;
                    break;
            }
            lastCursor = mouseCursor;
        }

        /// <summary>
        /// Returns the time elapsed since last frame render operation started.
        /// </summary>
        public static float SecondsSinceLastFrame
        {
            get { return 0; }
            //{
            //    //long curTicks = 0;
            //    //PerformanceTimer.QueryPerformanceCounter(ref curTicks);
            //    //float elapsedSeconds = (curTicks - CurrentFrameStartTicks) / (float)PerformanceTimer.TicksPerSecond;
            //    //return elapsedSeconds;
            //}
        }

        public bool IsPainting
        {
            get
            {
                return this.isPainting;
            }
        }

        public bool Repaint
        {
            get
            {
                return this.repaint;
            }
            set
            {
                this.repaint = value;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            foreach (IDisposable font in fontList.Values)
            {
                if (font != null)
                {
                    font.Dispose();
                }
            }
            fontList.Clear();

            if (measureCursor != null)
            {
                measureCursor.Dispose();
                measureCursor = null;
            }



            GC.SuppressFinalize(this);
        }

        #endregion

        public bool FogEnable { get; set; }
    }

    /// <summary>
    /// Mouse cursor
    /// </summary>
    public enum CursorType
    {
        Arrow = 0,
        Hand,
        Cross,
        Measure,
        SizeWE,
        SizeNS,
        SizeNESW,
        SizeNWSE
    }
}
