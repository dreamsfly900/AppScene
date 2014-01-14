using System;
using System.Collections.Generic;
using System.Text;

using System.Drawing;
using WorldWind.Renderable;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;


namespace AppScene
{
    /// <summary>
    /// 下面示例演示如何在屏幕上渲染一个圆
    /// 注意:顶点格式为TransformedColored
    /// </summary>
    public class Circle : RenderableObject
    {
        private Vector3 m_center;//圆心
        private float m_radius;//半径
        private List<Microsoft.DirectX.Direct3D.CustomVertex.TransformedColored> m_vertices = new List<Microsoft.DirectX.Direct3D.CustomVertex.TransformedColored>();//顶点列表
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="center">圆心(x,y,0)</param>
        /// <param name="radius">半径</param>
        public Circle(string name, Vector3 center, float radius)
            : base(name)
        {
            this.m_center = center;
            this.m_radius = radius;
        }

        /// <summary>
        /// 初始化对象
        /// </summary>
        /// <param name="drawArgs">渲染参数</param>
        public override void Initialize(DrawArgs drawArgs)
        {
            //将圆36等分
            double angle = Math.PI / 18;
            //圆心，注意屏幕坐标点用TransformedColored格式

            Vector4 ver = new Vector4();
            ver.X = m_center.X;
            ver.Y = m_center.Y;
            ver.Z = 0;
            CustomVertex.TransformedColored center = new CustomVertex.TransformedColored(ver, Color.FromArgb(100, Color.Red).ToArgb());
            //ver.W
            //center.Position.X = this.m_center.X;
            //center.Position.Y = this.m_center.Y;
            //center.Position.Z = 0;
            //center.Color = Color.FromArgb(100, Color.Red).ToArgb();
            this.m_vertices.Add(center);
            //圆边缘点集合
            for (int i = 0; i < 36; i++)
            {
                double x = this.m_center.X + this.m_radius * Math.Cos(i * angle);
                double y = this.m_center.Y + this.m_radius * Math.Sin(i * angle);
                CustomVertex.TransformedColored point = new CustomVertex.TransformedColored();
                point.Position = new Vector4((float)x, (float)y, 0.0f, 0.0f);
                point.Color = Color.FromArgb(100, Color.Red).ToArgb();
                this.m_vertices.Add(point);
            }
            //重复圆边缘第一个点
            Microsoft.DirectX.Direct3D.CustomVertex.TransformedColored firstPoint = new Microsoft.DirectX.Direct3D.CustomVertex.TransformedColored();

            Vector4 tempvec = new Vector4();
            tempvec.X = (float)(this.m_center.X + this.m_radius * Math.Cos(0));
            tempvec.Y = (float)(this.m_center.Y + this.m_radius * Math.Sin(0));
            tempvec.Z = 0;
            firstPoint.Position = tempvec;
            firstPoint.Color = Color.FromArgb(100, Color.Red).ToArgb();
            this.m_vertices.Add(firstPoint);
            this.isInitialized = true;
        }

        /// <summary>
        /// 渲染对象
        /// </summary>
        /// <param name="drawArgs">渲染参数</param>
        public override void Render(DrawArgs drawArgs)
        {
            if (!this.IsOn || !this.Initialized) return;
            //获取顶点格式
            VertexFormats format = drawArgs.Device.VertexFormat;
            //获取世界矩阵
            Matrix world = drawArgs.Device.GetTransform(TransformType.World);
            //获取纹理状态参数
            int colorOper = drawArgs.Device.GetTextureStageStateInt32(0, TextureStageStates.ColorOperation);
            //获取Z缓存
            int zbuffer = drawArgs.Device.GetRenderStateInt32(RenderStates.ZEnable);
            try
            {
                //设置顶点格式
                drawArgs.Device.VertexFormat = CustomVertex.TransformedColored.Format;
                //设置Z缓存
                drawArgs.Device.SetRenderState(RenderStates.ZEnable, true);
                //设置纹理状态，此处未使用纹理
                drawArgs.Device.SetTextureStageState(0, TextureStageStates.ColorOperation, (int)TextureOperation.Disable);
                ////绘制圆
                drawArgs.Device.DrawUserPrimitives(PrimitiveType.TriangleFan, m_vertices.Count - 2, m_vertices.ToArray());
            }
            catch (Exception e)
            {
                Utility.Log.Write(e);
            }
            finally
            {
                //恢复各个渲染状态和相关矩阵
                drawArgs.Device.VertexFormat = format;
                drawArgs.Device.SetRenderState(RenderStates.ZEnable, zbuffer);
                drawArgs.Device.SetTransform(TransformType.World, world);
                drawArgs.Device.SetTextureStageState(0, TextureStageStates.ColorOperation, colorOper);
                drawArgs.Device.Indices = null;
            }
        }

        /// <summary>
        /// 更新对象
        /// </summary>
        /// <param name="drawArgs">渲染参数</param>
        public override void Update(DrawArgs drawArgs)
        {
            if (!this.Initialized)
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
            //base.Dispose();
        }



        public override bool PerformSelectionAction(DrawArgs drawArgs)
        {
            throw new NotImplementedException();
        }
    }
}
