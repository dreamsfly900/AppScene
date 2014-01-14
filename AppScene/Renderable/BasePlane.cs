using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WorldWind.Renderable;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;
using System.Drawing;

namespace AppScene
{
    class BasePlane : RenderableObject
    {
        public BasePlane(string name)
            : base(name)
        {
           
        }
        public override void Initialize(DrawArgs drawArgs)
        {
            this.isInitialized = true;
            m_VertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionColored), 6, drawArgs.Device, 0, CustomVertex.PositionColored.Format, Pool.Default);
            m_IndexBuffer=new IndexBuffer(typeof(int),18,drawArgs.Device,0,Pool.Default);
            CumptureVertexs();
        }
        protected VertexBuffer m_VertexBuffer;
        protected IndexBuffer m_IndexBuffer;
        private void CumptureVertexs()
        {
            CustomVertex.PositionColored[] vertices = (
CustomVertex.PositionColored[]) m_VertexBuffer.Lock(0,0);//定义顶点 
            vertices[0].Position = new Vector3(-2.0f,-2.0f, -2.0f);
            vertices[0].Color = Color.Red.ToArgb();
            vertices[1].Position = new Vector3(-2.0f, -2.0f, 2.0f);
            vertices[1].Color = Color.Green.ToArgb();
            vertices[2].Position = new Vector3(2.0f, -2.0f, -2.0f);
            vertices[2].Color = Color.Yellow.ToArgb();
            vertices[3].Position = new Vector3(-2.0f, -2.0f, -2.0f);
            vertices[3].Color = Color.Red.ToArgb();
            vertices[4].Position = new Vector3(2.0f, -2.0f, 2.0f);
            vertices[4].Color = Color.Green.ToArgb();
            vertices[5].Position = new Vector3(2.0f, -2.0f, -2.0f);
            vertices[5].Color = Color.Yellow.ToArgb();
            m_VertexBuffer.Unlock();

            int[] index = (int[])m_IndexBuffer.Lock(0, 0) ;
            for (int i = 0; i < 6; i++)
            {
                index[3 * i] = i;
                index[3 * i + 1] = i + 1;
                index[3 * i + 2] = i + 2;
            }
            m_IndexBuffer.Unlock();
           
        }

        public override void Update(DrawArgs drawArgs)
        {
            if (!isInitialized && isOn)
            {
                Initialize(drawArgs);
            }
        }

        public override void Render(DrawArgs drawArgs)
        {
            if (!isInitialized || !isOn)
                return;
            //(1)设置顶点格式
            drawArgs.Device.VertexFormat = Microsoft.DirectX.Direct3D.CustomVertex.PositionColored.Format;//顶点格式
            drawArgs.Device.SetStreamSource(0, m_VertexBuffer, 0);
            drawArgs.Device.Indices = m_IndexBuffer;
            drawArgs.Device.DrawUserPrimitives(PrimitiveType.TriangleList, 1,2 );
            drawArgs.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 0, 0, 12);//圆锥上面的盖子
           
        }

        public override void Dispose()
        {
            if (m_VertexBuffer != null)
            {
                m_VertexBuffer.Dispose();
                m_VertexBuffer = null;
            }
            if (m_IndexBuffer != null)
            {
                m_IndexBuffer.Dispose();
                m_IndexBuffer = null;
            }
        }

        public override bool PerformSelectionAction(DrawArgs drawArgs)
        {
            return true;
           // throw new NotImplementedException();
        }
    }
}
