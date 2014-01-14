using System;
using System.Collections.Generic;
using System.Text;
using WorldWind.Renderable;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;

namespace AppScene
{
    /// <summary>
    /// 此示例主要演示如何构造一个mesh网格,以及实现碰撞检测
    /// </summary>
    public class Sphere : RenderableObject
    {
        #region 私有变量
        private Vector3 m_center;//球体球心(模型坐标)
        private float m_radius;//球体半径
        private short m_slices;//球体在水平方向的分块数目
        private short m_stacks;//球体在竖直方向的分块数目
        private CustomVertex.PositionColored[] vertices;//定义球体网格顶点
        private short[] indices;//定义球体网格中三角形索引
        private Mesh mesh;//球体mesh网格
        #endregion

        /// <summary>
        /// 构造啊函数
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="param">模型参数</param>
        /// <param name="radius">球体半径</param>
        /// <param name="slices">球体在水平方向的分块数目</param>
        /// <param name="stacks">球体在竖直方向的分块数目</param>
        public Sphere(string name, float radius, short slices, short stacks)
            : base(name)
        {
            this.m_radius = radius;
            this.m_slices = slices;
            this.m_stacks = stacks;
        }

        #region 构造球体
        /// <summary>
        /// 计算顶点
        /// </summary>
        /// <remarks>
        /// 球体上任意一点坐标可以通过球形坐标来表示(r半径，theta垂直角，alpha水平角)
        /// X=r*sin(theta)*cos(alpha);
        /// Y=r*cos(theta);
        /// Z=r*sin(theta)*sin(alpha);
        /// </remarks>
        private void ComputeVertexs()
        {
            vertices = new CustomVertex.PositionColored[(m_stacks + 1) * (m_slices + 1)];
            float theta = (float)Math.PI / m_stacks;
            float alpha = 2 * (float)Math.PI / m_slices;
            for (int i = 0; i < m_slices + 1; i++)
            {
                for (int j = 0; j < m_stacks + 1; j++)
                {
                    Vector3 pt = new Vector3();
                    pt.X = m_center.X + m_radius * (float)Math.Sin(i * theta) * (float)Math.Cos(j * alpha);
                    pt.Y = m_center.Y + m_radius * (float)Math.Cos(i * theta);
                    pt.Z = m_center.Z + m_radius * (float)Math.Sin(i * theta) * (float)Math.Sin(j * alpha);
                    vertices[j + i * (m_stacks + 1)].Position = pt;
                    vertices[j + i * (m_stacks + 1)].Color = Color.FromArgb(200, Color.Blue).ToArgb();
                }
            }
        }

        /// <summary>
        /// 计算索引
        /// </summary>
        private void ComputeIndices()
        {
            indices = new short[6 * m_stacks * m_slices];
            for (short i = 0; i < m_slices; i++)
            {
                for (short j = 0; j < m_stacks; j++)
                {
                    indices[6 * (j + i * m_stacks)] = (short)(j + i * (m_stacks + 1));
                    indices[6 * (j + i * m_stacks) + 1] = (short)(j + i * (m_stacks + 1) + 1);
                    indices[6 * (j + i * m_stacks) + 2] = (short)(j + (i + 1) * (m_stacks + 1));
                    indices[6 * (j + i * m_stacks) + 3] = (short)(j + i * (m_stacks + 1) + 1);
                    indices[6 * (j + i * m_stacks) + 4] = (short)(j + (i + 1) * (m_stacks + 1) + 1);
                    indices[6 * (j + i * m_stacks) + 5] = (short)(j + (i + 1) * (m_stacks + 1));
                }
            }
        }

        #endregion

        #region Renderable
        /// <summary>
        /// 初始化对象
        /// </summary>
        /// <param name="drawArgs">渲染参数</param>
        public override void Initialize(DrawArgs drawArgs)
        {
            //计算模型位置
            //this.ComputePosition(drawArgs, this.m_modelParam);
            ////计算转换矩阵
            //this.ComputeWorldTransform(this.m_modelParam);
            //球体球心
            this.m_center = new Vector3();
            m_center.X = 0;
            m_center.Y = 0;
            m_center.Z = 0;
            ComputeVertexs();//计算顶点
            ComputeIndices();//计算索引

            //构造mesh
            mesh = new Mesh(indices.Length / 3, vertices.Length, MeshFlags.Managed, CustomVertex.PositionColored.Format, drawArgs.Device);

            //顶点缓冲
            GraphicsStream vs = mesh.LockVertexBuffer(LockFlags.None);
            vs.Write(vertices);
            mesh.UnlockVertexBuffer();
            vs.Dispose();

            //索引缓冲
            GraphicsStream ids = mesh.LockIndexBuffer(LockFlags.None);
            ids.Write(indices);
            mesh.UnlockIndexBuffer();
            ids.Dispose();

            this.isInitialized = true;
        }

        /// <summary>
        /// 渲染对象
        /// </summary>
        /// <param name="drawArgs">渲染参数</param>
        public override void Render(DrawArgs drawArgs)
        {
            if (!this.IsOn || !this.isInitialized) return;

            //获取当前世界变换
            Matrix world = drawArgs.Device.GetTransform(TransformType.World);
            //获取当前顶点格式
            VertexFormats format = drawArgs.Device.VertexFormat;
            //获取当前的Z缓冲方式
            int zEnable = drawArgs.Device.GetRenderStateInt32(RenderStates.ZEnable);
            //获取纹理状态
            int colorOper = drawArgs.Device.GetTextureStageStateInt32(0, TextureStageStates.ColorOperation);

            try
            {
                //计算模型的转换矩阵
                Matrix m = this.WorldTransform;
                m = drawArgs.Device.GetTransform(TransformType.World);
               // m *= Matrix.Translation(this.m_position - drawArgs.WorldCamera.ReferenceCenter);
                m *= Matrix.Translation(this.m_position);
                //设置世界矩阵
                drawArgs.Device.SetTransform(TransformType.World, m);
                //获取当前转换矩阵，主要用于选择操作时，顶点的转换
                //设置顶点格式
                drawArgs.Device.VertexFormat = CustomVertex.PositionColored.Format;
                //设置Z缓冲
                drawArgs.Device.SetRenderState(RenderStates.ZEnable, true);
                //设置纹理状态，此处未使用纹理
                drawArgs.Device.SetTextureStageState(0, TextureStageStates.ColorOperation, (int)TextureOperation.Disable);
                //绘制mesh网格
                mesh.DrawSubset(0);
            }
            catch (Exception e)
            {
                Utility.Log.Write(e);
            }
            finally
            {
                drawArgs.Device.SetTransform(TransformType.World, world);
                drawArgs.Device.VertexFormat = format;
                drawArgs.Device.SetRenderState(RenderStates.ZEnable, zEnable);
                drawArgs.Device.SetTextureStageState(0, TextureStageStates.ColorOperation, colorOper);
            }
        }

        /// <summary>
        /// 更新对象
        /// </summary>
        /// <param name="drawArgs">渲染参数</param>
        public override void Update(DrawArgs drawArgs)
        {
            if (!this.isInitialized)
            {
                this.Initialize(drawArgs);
            }
            // base.Update(drawArgs);
        }

        /// <summary>
        /// 执行选择操作
        /// </summary>
        /// <param name="X">点选X坐标</param>
        /// <param name="Y">点选Y坐标</param>
        /// <param name="drawArgs">渲染参数</param>
        /// <returns>选择返回True,否则返回False</returns>
        public bool PerformSelectionAction(int X, int Y, DrawArgs drawArgs)
        {
            if (!this.isInitialized) return false;
            Vector3 v1 = new Vector3();
            v1.X = X;
            v1.Y = Y;
            v1.Z = 0;
            Vector3 v2 = new Vector3();
            v2.X = X;
            v2.Y = Y;
            v2.Z = 1;
            //将屏幕坐标装换为世界坐标，构造一个射线
            Vector3 rayPos = drawArgs.WorldCamera.UnProject(v1);
            Vector3 rayDir = drawArgs.WorldCamera.UnProject(v2) - rayPos;
            //判断模型是否与射线相交
            bool result = this.IntersectWithRay(rayPos, rayDir, drawArgs);
            //if (result)
            // drawArgs.Selection.Add(this);
            return result;
        }

        /// <summary>
        /// 判断模型是否与射线相交
        /// </summary>
        /// <param name="rayPos">射线原点</param>
        /// <param name="rayDir">射线方向</param>
        /// <param name="drawArgs">绘制参数</param>
        /// <returns>如果相交返回True,否则返回False</returns>
        public  bool IntersectWithRay(Vector3 rayPos, Vector3 rayDir, DrawArgs drawArgs)
        {
            //if (!this.IsOn || !this.isInitialized || !this.IsSelectable) return false;
            bool isSelected = false;
            //try
            //{
            //    //选中距离
            //    float dis;
            //    //构造一条基于模型本体坐标系的射线，用于判断射线是否与模型相交
            //    Matrix invert = Matrix.Invert(this.WorldTransform.Matrix3d);
            //    Vector3 rayPos1 = Vector3.TransformCoordinate(rayPos, invert);
            //    Vector3 rayPos2 = rayPos + rayDir;
            //    rayPos2 = Vector3.TransformCoordinate(rayPos2, invert);
            //    Vector3 rayDir1 = rayPos2 - rayPos1;
            //    Ray ray1 = new Ray(rayPos1, rayDir1);
            //    isSelected = mesh.Intersects(ray1, out dis);
            //    this.m_selectedMinDistance = dis;
            //}
            //catch (Exception caught)
            //{
            //    Utility.Log.Write(caught);
            //}
            return isSelected;
        }

        /// <summary>
        /// 释放对象
        /// </summary>
        public override void Dispose()
        {
            if (this.mesh != null)
                this.mesh.Dispose();
            this.isInitialized = false;
            //base.Dispose();
        }
        #endregion

        public Vector3 m_position { get; set; }

        public Matrix WorldTransform { get; set; }

        public override bool PerformSelectionAction(DrawArgs drawArgs)
        {
            throw new NotImplementedException();
        }
    }
}

