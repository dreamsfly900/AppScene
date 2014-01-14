using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WorldWind.Renderable;
using Utility;
using Microsoft.DirectX.Direct3D;
using System.IO;
using Microsoft.DirectX;
using System.Drawing;
using System.Windows.Forms;

namespace AppScene
{
    public class Tri : RenderableObject
    {
        static Effect m_effect = null;
        VertexBuffer vertexBuffer = null;
        public Tri(string name)
            : base(name)
        {
        }
        public override void Initialize(DrawArgs drawArgs)
        {
            if (m_effect == null)
            {
                string outerrors = "";
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                Stream effectStream = assembly.GetManifestResourceStream("AppScene.Tri.fx");
                string pathfx = "Tris.fx";
                // string pathfx = " Default_DirectX_Effect.fx";
                //string pathfx = "CreateParamModel.fx";

                //string pathfx = "flag.fx";
                //m_effect = Effect.FromStream(
                //    drawArgs.device,
                //    effectStream,
                //    null,
                //    null,
                //    ShaderFlags.None,
                //    null,
                //    out outerrors);
                m_effect = Effect.FromFile(
                    drawArgs.Device,
                    pathfx,
                    null,
                    null,
                    ShaderFlags.None,
                    null,
                    out outerrors);
                if (outerrors != null && outerrors.Length > 0)
                    Log.Write(Log.Levels.Error, outerrors);
            }
            vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionColored), 3, drawArgs.Device, 0, CustomVertex.PositionColored.Format, Pool.Default);
            vertexBuffer.Created += new EventHandler(vertexBuffer_Created);
            vertexBuffer_Created(vertexBuffer, null);
            Matrix WorldMatrix = Matrix.Identity;
            Matrix viewMatrix = Matrix.LookAtLH(new Vector3(0.0f, 3.0f, -9.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f));
            Matrix projMatrix = Matrix.PerspectiveFovLH((float)Math.PI / 4, 1.0f, 1.0f, 100.0f);
            WorldMatrix = drawArgs.Device.GetTransform(TransformType.World);
            viewMatrix = drawArgs.Device.GetTransform(TransformType.View);
            projMatrix = drawArgs.Device.GetTransform(TransformType.Projection);

            m_effect.SetValue("worldViewProjection", WorldMatrix * viewMatrix * projMatrix);
            // m_effect.SetValue("matViewProjection", viewMatrix * projMatrix); 
            isInitialized = true;
        }

        public override void Update(DrawArgs drawArgs)
        {
            try
            {
                if (!isInitialized)
                    Initialize(drawArgs);

            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }
        public override void Render(DrawArgs drawArgs)
        {
            if (!isInitialized)
                return;

            drawArgs.Device.SetStreamSource(0, vertexBuffer, 0);
            drawArgs.Device.VertexFormat = CustomVertex.PositionColored.Format;
            int iTime = Environment.TickCount % 1000;
            float Angle = iTime * (2.0f * (float)Math.PI) / 1000.0f;
            m_effect.SetValue("Time", Angle);
            m_effect.Technique = "RenderScene";
            // m_effect.Technique = "Default_DirectX_Effect";
            int numPasses = m_effect.Begin(0);

            for (int i = 0; i < numPasses; i++)
            {
                m_effect.BeginPass(i);
                drawArgs.Device.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);
                m_effect.EndPass();
            }
            //System.Threading.Thread.Sleep(200);
            m_effect.End();

        }

        void vertexBuffer_Created(object sender, EventArgs e)
        {
            CustomVertex.PositionColored[] verts = (CustomVertex.PositionColored[])vertexBuffer.Lock(0, 0);
            verts[0].Position = new Vector3(0.0f, 1.0f, 0.0f);
            verts[0].Color = Color.Red.ToArgb();
            verts[1].Position = new Vector3(0.5f, 1.0f, 0.0f);
            verts[1].Color = Color.Red.ToArgb();
            verts[2].Position = new Vector3(1.0f, 1.0f, 1.0f);
            verts[2].Color = Color.Black.ToArgb();
            vertexBuffer.Unlock();
        }

        public override void Dispose()
        {
            if (vertexBuffer != null && vertexBuffer.Disposed == false)
            {
                vertexBuffer.Dispose();
                vertexBuffer = null;
            }
            if (m_effect != null && m_effect.Disposed == false)
            {
                m_effect.Dispose();
                m_effect = null;
            }
        }

        public override bool PerformSelectionAction(DrawArgs drawArgs)
        {
            return false;
        }
    }
}
