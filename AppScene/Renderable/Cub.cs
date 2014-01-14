using System;
using System.Collections.Generic;
using System.Text;
using WorldWind.Renderable;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;


namespace AppScene
{
    public class Cub : RenderableObject
    {
        private Vector3[] m_points;//顶点
        private CustomVertex.PositionColored[] m_vertices;//顶点数组
        private int[] m_indices;//索引数组
        private VertexBuffer m_vertexBuffer;//顶点缓冲
        private IndexBuffer m_indexBuffer;//索引缓冲
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="points">顶点数组</param>
        public Cub(string name, Vector3[] points)
            : base(name)
        {
            m_points = points;
        }


        /// <summary>
        /// 初始化对象
        /// </summary>
        /// <param name="drawArgs">渲染参数</param>
        public override void Initialize(DrawArgs drawArgs)
        {
            ComputeVertices();
            ComputeIndices();
            m_vertexBuffer = new
VertexBuffer(typeof(CustomVertex.PositionColoredTextured), m_vertices.Length, drawArgs.Device, Usage.Dynamic | Usage.WriteOnly,
CustomVertex.PositionColoredTextured.Format, Pool.Default);

            m_indexBuffer = new IndexBuffer(typeof(int), m_indices.Length, drawArgs.Device,
Usage.WriteOnly, Pool.Default);
            //m_vertexBuffer = VertexBuffer.CreateBuffer(drawArgs.Device, m_vertices);
            //m_indexBuffer = new IndexBuffer() m_indices;
            m_vertexBuffer.SetData(m_vertices, 0, LockFlags.None);
            m_indexBuffer.SetData(m_indices, 0, LockFlags.None);
            this.isInitialized = true;
            //  base.Initialize(drawArgs);
        }
        /// <summary>
        /// 计算顶点
        /// </summary>
        public void ComputeVertices()
        {
            m_vertices = new CustomVertex.PositionColored[m_points.Length];
            for (int i = 0; i < m_points.Length; i++)
            {
                Vector3 vec = new Vector3(m_points[i].Y, m_points[i].X, m_points[i].Z);
                m_vertices[i].Position = vec;// MathEngine.SphericalToCartesian(+WorldSettings.EquatorialRadius);
                if (i < 4)
                {
                    m_vertices[i].Color = Color.FromArgb(120, Color.Red).ToArgb();
                }
                else
                {
                    m_vertices[i].Color = Color.FromArgb(120, Color.SeaGreen).ToArgb();
                }

            }
        }
        /// <summary>
        /// 计算索引
        /// </summary>
        public void ComputeIndices()
        {
            m_indices = new int[36];
            //顶面
            m_indices[0] = 0; m_indices[1] = 1; m_indices[2] = 3;
            m_indices[3] = 1; m_indices[4] = 2; m_indices[5] = 3;
            //底面
            m_indices[6] = 4; m_indices[7] = 5; m_indices[8] = 7;
            m_indices[9] = 5; m_indices[10] = 6; m_indices[11] = 7;
            //前面
            m_indices[12] = 1; m_indices[13] = 5; m_indices[14] = 2;
            m_indices[15] = 5; m_indices[16] = 6; m_indices[17] = 2;
            //后面
            m_indices[18] = 0; m_indices[19] = 4; m_indices[20] = 3;
            m_indices[21] = 4; m_indices[22] = 7; m_indices[23] = 3;
            //左面
            m_indices[24] = 0; m_indices[25] = 4; m_indices[26] = 5;
            m_indices[27] = 0; m_indices[28] = 5; m_indices[29] = 1;
            //右表面
            m_indices[30] = 3; m_indices[31] = 7; m_indices[32] = 6;
            m_indices[33] = 3; m_indices[34] = 2; m_indices[35] = 6;
        }
        float ang = 0.0f;
        /// <summary>
        /// 渲染对象
        /// </summary>
        /// <param name="drawArgs">渲染参数</param>
        public override void Render(DrawArgs drawArgs)
        {
            if (!this.isOn || !this.isInitialized) return;
            //获取顶点格式
            VertexFormats format = drawArgs.Device.VertexFormat;
            //获取世界矩阵
            Matrix matrix = drawArgs.Device.GetTransform(TransformType.World);
            //获取纹理状态
            int colorOper = drawArgs.Device.GetTextureStageStateInt32(0, TextureStageStates.ColorOperation);
            //获取z缓存
            int zbuffer = drawArgs.Device.GetRenderStateInt32(RenderStates.ZEnable);
            try
            {
                ang +=(float) Math.PI/1800;
               
                //设置顶点格式
                drawArgs.Device.VertexFormat = CustomVertex.PositionColored.Format;
                //设置z缓存
                drawArgs.Device.SetRenderState(RenderStates.ZEnable, false);
                Matrix matrix0 = Matrix.Identity;
                matrix0.RotateY(ang);
                //设置世界矩阵
               // drawArgs.Device.SetTransform(TransformType.World, Matrix.Translation(new Vector3(0.0f, 10.0f, 0.0f)));
                drawArgs.Device.SetTransform(TransformType.World, matrix0);
                //设置纹理状态
                drawArgs.Device.SetTextureStageState(0, TextureStageStates.ColorOperation, (int)TextureOperation.Disable);
                //设置顶点缓存
                drawArgs.Device.SetStreamSource(0, m_vertexBuffer, 0, CustomVertex.PositionColored.StrideSize);
                //设置索引
                drawArgs.Device.Indices = m_indexBuffer;
                //绘制对象
                drawArgs.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, m_vertices.Length, 0, m_indices.Length / 3);
            }
            catch (Exception e)
            {
                Utility.Log.Write(e);
            }
            finally
            {
                //恢复各个渲染状态参数
                drawArgs.Device.VertexFormat = format;
                drawArgs.Device.SetTransform(TransformType.World, matrix);
                drawArgs.Device.SetTextureStageState(0, TextureStageStates.ColorOperation, colorOper);
                drawArgs.Device.SetRenderState(RenderStates.ZEnable, zbuffer);
            }
        }

        /// <summary>
        /// 更新对象
        /// </summary>
        /// <param name="drawArgs">渲染参数</param>
        public override void Update(DrawArgs drawArgs)
        {
            if (!Initialized)
            {
                this.Initialize(drawArgs);
            }
            //base.Update(drawArgs);
        }

        /// <summary>
        /// 释放对象
        /// </summary>
        public override void Dispose()
        {
            if (m_indexBuffer != null && m_indexBuffer.Disposed)
            {
                m_indexBuffer.Dispose();
            }
            if (m_vertexBuffer != null && m_vertexBuffer.Disposed)
            {
                m_vertexBuffer.Dispose();
            }
            this.m_indexBuffer = null;
            this.isInitialized = false;
            //base.Dispose();
        }

        public override bool PerformSelectionAction(DrawArgs drawArgs)
        {
            throw new NotImplementedException();
        }
    }
}




