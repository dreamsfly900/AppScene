using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WorldWind.Renderable;
using Microsoft.DirectX;
using AppScene.Renderable;

namespace AppScene
{
    public partial class MainFrm : Form
    {
        public MainFrm()
        {
            InitializeComponent();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //demo01 demo001 = new demo01("你啊哈");
            //demo001.IsOn = true;
            //demo001.RenderPriority = RenderPriority.Custom;
            //this.sceneControl1.CurrentWorld.RenderableObjects.Add(demo001);

            //BasePlane demo002 = new BasePlane("你啊哈0");
            //demo002.IsOn = true;
            //demo002.RenderPriority = RenderPriority.Custom;
            //this.sceneControl1.CurrentWorld.RenderableObjects.Add(demo002);

            //Mountain Mountain1 = new Mountain("你啊哈1");
            //Mountain1.IsOn = true;
            //Mountain1.RenderPriority = RenderPriority.Custom;
            //this.sceneControl1.CurrentWorld.RenderableObjects.Add(Mountain1);

            Tri tr = new Tri("三角形");
            tr.IsOn = true;
            tr.RenderPriority = RenderPriority.Custom;
            this.sceneControl1.CurrentWorld.RenderableObjects.Add(tr);

            //Vector3 vec = new Vector3();
            //vec.X = 200;
            //vec.Y = 200;
            //vec.Z = 10;
            //Circle circle = new Circle("圆", vec, 100);
            //circle.IsOn = true;
            //circle.RenderPriority = RenderPriority.Custom;
            //this.sceneControl1.CurrentWorld.RenderableObjects.Add(circle);

            Vector3[] vecs = new Vector3[8];
            vecs[0] = new Vector3(0.0f, 0.0f, 0.0f);
            vecs[1] = new Vector3(1.0f, 0.0f, 0.0f);
            vecs[2] = new Vector3(1.0f, 1.0f, 0.0f);
            vecs[3] = new Vector3(0.0f, 1.0f, 0.0f);
            vecs[4] = new Vector3(0.0f, 0.0f, 2.0f);
            vecs[5] = new Vector3(1.0f, 0.0f, 2.0f);
            vecs[6] = new Vector3(1.0f, 1.0f, 2.0f);
            vecs[7] = new Vector3(0.0f, 1.0f, 2.0f);
            Cub cub = new Cub("立方体", vecs);
            cub.IsOn = true;
            cub.RenderPriority = RenderPriority.Custom;
            sceneControl1.CurrentWorld.RenderableObjects.Add(cub);
            Tiger tiger = new Tiger("老虎");
            tiger.IsOn = true;
            tiger.RenderPriority = RenderPriority.Custom;
            sceneControl1.CurrentWorld.RenderableObjects.Add(tiger);

            //Sphere sph = new Sphere("", 2, 24, 30);
            //sph.IsOn = true;
            //sph.RenderPriority = RenderPriority.Custom;
            //sceneControl1.CurrentWorld.RenderableObjects.Add(sph);

            //Earth sph = new Earth("地球", 5, 72, 72);
            //sph.Selected += new Earth.IsSelected(Select);
            //sph.IsOn = true;
            //sph.RenderPriority = RenderPriority.Custom;
            //sceneControl1.CurrentWorld.RenderableObjects.Add(sph);
        }
        void Select(float dd)
        {
            MessageBox.Show(dd.ToString());
        }
    }
}
