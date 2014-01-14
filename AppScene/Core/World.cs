using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WorldWind.Renderable;
using Utility;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;
using System.Drawing;

namespace AppScene
{
    public class World : RenderableObject
    {
        RenderableObjectList _renderableObjects;
        public RenderableObjectList RenderableObjects
        {
            get
            {
                return this._renderableObjects;
            }
            set
            {
                this._renderableObjects = value;
            }
        }
        public World(string str)
            : base(str)
        {
            this._renderableObjects = new RenderableObjectList(this.Name);
        }
        public override void Initialize(DrawArgs drawArgs)
        {
            try
            {
                if (this.isInitialized)
                    return;

                this.RenderableObjects.Initialize(drawArgs);
            }
            catch (Exception caught)
            {
                Log.DebugWrite(caught);
            }
            finally
            {
                this.isInitialized = true;
            }
        }
        public override void Update(DrawArgs drawArgs)
        {
            if (!this.isInitialized)
            {
                this.Initialize(drawArgs);
            }

            if (this.RenderableObjects != null)
            {
                this.RenderableObjects.Update(drawArgs);
            }

        }

        public override bool PerformSelectionAction(DrawArgs drawArgs)
        {
            return this._renderableObjects.PerformSelectionAction(drawArgs);
        }

        public override void Render(DrawArgs drawArgs)
        {
            try
            {
                CustomVertex.PositionColored[] vertices2 = new
 CustomVertex.PositionColored[3];//定义顶点 
                vertices2[0].Position = new Vector3(0f, 0f, 10f);
                vertices2[0].Color = Color.Red.ToArgb();
                vertices2[1].Position = new Vector3(0, 1f, 10f);
                vertices2[1].Color = Color.Green.ToArgb();
                vertices2[2].Position = new Vector3(1f, 0f, 10f);
                vertices2[2].Color = Color.Yellow.ToArgb();
                drawArgs.Device.VertexFormat = CustomVertex.PositionColored.Format;
                drawArgs.Device.DrawUserPrimitives(PrimitiveType.TriangleList, 1, vertices2);

                //RenderStars(drawArgs, RenderableObjects);

                //if (drawArgs.CurrentWorld.IsEarth && World.Settings.EnableAtmosphericScattering)
                //{
                //    float aspectRatio = (float)drawArgs.WorldCamera.Viewport.Width / drawArgs.WorldCamera.Viewport.Height;
                //    float zNear = (float)drawArgs.WorldCamera.Altitude * 0.1f;
                //    double distToCenterOfPlanet = (drawArgs.WorldCamera.Altitude + equatorialRadius);
                //    double tangentalDistance = Math.Sqrt(distToCenterOfPlanet * distToCenterOfPlanet - equatorialRadius * equatorialRadius);
                //    double amosphereThickness = Math.Sqrt(m_outerSphere.m_radius * m_outerSphere.m_radius + equatorialRadius * equatorialRadius);
                //    Matrix proj = drawArgs.device.Transform.Projection;
                //    drawArgs.device.Transform.Projection = Matrix.PerspectiveFovRH((float)drawArgs.WorldCamera.Fov.Radians, aspectRatio, zNear, (float)(tangentalDistance + amosphereThickness));
                //    drawArgs.device.RenderState.ZBufferEnable = false;
                //    drawArgs.device.RenderState.CullMode = Cull.CounterClockwise;
                //    m_outerSphere.Render(drawArgs);
                //    drawArgs.device.RenderState.CullMode = Cull.Clockwise;
                //    drawArgs.device.RenderState.ZBufferEnable = true;

                //    drawArgs.device.Transform.Projection = proj;
                //}

                //if (World.EnableSunShading)
                //    RenderSun(drawArgs);

                //render SurfaceImages
                Render(RenderableObjects, WorldWind.Renderable.RenderPriority.TerrainMappedImages, drawArgs);

                //if (m_projectedVectorRenderer != null)
                //    m_projectedVectorRenderer.Render(drawArgs);


                //render Placenames
                Render(RenderableObjects, WorldWind.Renderable.RenderPriority.Placenames, drawArgs);

                //绘制自定义渲染对象
                Render(RenderableObjects, WorldWind.Renderable.RenderPriority.Custom, drawArgs);
                showPlanetAxis = true;
                //绘制坐标轴
                if (showPlanetAxis)
                    this.DrawAxis(drawArgs);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }
        private void Render(WorldWind.Renderable.RenderableObject renderable, WorldWind.Renderable.RenderPriority priority, DrawArgs drawArgs)
        {
            if (!renderable.IsOn || (renderable.Name != null && renderable.Name.Equals("Starfield")))
                return;
            try
            {
                if (renderable is WorldWind.Renderable.RenderableObjectList)
                {
                    WorldWind.Renderable.RenderableObjectList rol = (WorldWind.Renderable.RenderableObjectList)renderable;
                    for (int i = 0; i < rol.ChildObjects.Count; i++)
                    {
                        Render((WorldWind.Renderable.RenderableObject)rol.ChildObjects[i], priority, drawArgs);
                    }
                }
                // hack at the moment
                else if (priority == WorldWind.Renderable.RenderPriority.TerrainMappedImages)
                {
                    if (renderable.RenderPriority == WorldWind.Renderable.RenderPriority.SurfaceImages || renderable.RenderPriority == WorldWind.Renderable.RenderPriority.TerrainMappedImages)
                    {
                        renderable.Render(drawArgs);
                    }
                }
                else if (renderable.RenderPriority == priority)
                {
                    renderable.Render(drawArgs);
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }
        private void DrawAxis(DrawArgs drawArgs)
        {
            drawArgs.Device.VertexFormat = CustomVertex.PositionColored.Format;
            drawArgs.Device.TextureState[0].ColorOperation = TextureOperation.Disable;
            //drawArgs.device.Transform.World = Matrix.Translation(
            //    (float)-drawArgs.WorldCamera.ReferenceCenter.X,
            //    (float)-drawArgs.WorldCamera.ReferenceCenter.Y,
            //    (float)-drawArgs.WorldCamera.ReferenceCenter.Z
            //    );
            CustomVertex.PositionColored[] axisX = new CustomVertex.PositionColored[2];

            axisX[0].X = 0;
            axisX[0].Y = 0;
            axisX[0].Z = 0;
            axisX[0].Color = System.Drawing.Color.Red.ToArgb();
            axisX[1].X = 10;
            axisX[1].Y = 0;
            axisX[1].Z = 0;
            axisX[1].Color = System.Drawing.Color.Red.ToArgb();
            drawArgs.Device.DrawUserPrimitives(PrimitiveType.LineStrip, 1, axisX);
            CustomVertex.PositionColored[] axisY = new CustomVertex.PositionColored[2];

            axisY[0].X = 0;
            axisY[0].Y = 0;
            axisY[0].Z = 0;
            axisY[0].Color = System.Drawing.Color.Green.ToArgb();
            axisY[1].X = 0;
            axisY[1].Y = 10;
            axisY[1].Z = 0;
            axisY[1].Color = System.Drawing.Color.Green.ToArgb();
            drawArgs.Device.DrawUserPrimitives(PrimitiveType.LineStrip, 1, axisY);

            CustomVertex.PositionColored[] axisZ = new CustomVertex.PositionColored[2];

            axisZ[0].X = 0;
            axisZ[0].Y = 0;
            axisZ[0].Z = 0;
            axisZ[0].Color = System.Drawing.Color.Yellow.ToArgb();
            axisZ[1].X = 0;
            axisZ[1].Y = 0;
            axisZ[1].Z = 10;
            axisZ[1].Color = System.Drawing.Color.Yellow.ToArgb();
            drawArgs.Device.DrawUserPrimitives(PrimitiveType.LineStrip, 1, axisZ);
            //drawArgs.Device.Transform.World = drawArgs.WorldCamera.mWorldMatrix;

        }
        public override void Dispose()
        {
            if (this.RenderableObjects != null)
            {
                this.RenderableObjects.Dispose();
                this.RenderableObjects = null;
            }

        }



        public bool showPlanetAxis { get; set; }

        public static bool EnableSunShading { get; set; }
    }
}
