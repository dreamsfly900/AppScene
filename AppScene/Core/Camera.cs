using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.PrivateImplementationDetails;
using Microsoft.DirectX.Direct3D;

namespace AppScene
{
    public enum CameraType { LANDOBJECT, AIRCRAFT };
    public class Camera
    {
        CameraType mCameraType;
        Vector3 mPosition; //相机位置
        Vector3 mLook;//LookVector  
        Vector3 mUp;// UpVector  
        Vector3 mRight;// RightVector  
        Vector3 ViewFrustum;// 平面截投体  

        protected Viewport mViewPort;//视口大小
        protected Matrix m_ProjectionMatrix; //上一次渲染采用的投影变换矩阵 Projection matrix used in last render.
        protected Matrix m_ViewMatrix; //上一次渲染采用的观察矩阵 View matrix used in last render.
        protected Matrix m_WorldMatrix = Matrix.Identity;//世界变换矩阵
        public Camera()
        {
            mCameraType = CameraType.AIRCRAFT;
            mPosition = new Vector3(0.0f, 0.0f, -50.0f);//注意默认位置，现在对了。
            mRight = new Vector3(0.0f, 1.0f, 0.0f);
            mUp = new Vector3(0.0f, 1.0f, 0.0f);
            mLook = new Vector3(0.0f, 0.0f, 10.0f);
        }
        public Camera(Vector3 cameraPosition, Vector3 cameraTarget, Vector3 upVector)
        {
            mCameraType = CameraType.AIRCRAFT;
            mPosition = new Vector3(0.0f, 0.0f, -100.0f);
            mRight = new Vector3(1.0f, 0.0f, 0.0f);
            mUp = new Vector3(0.0f, 1.0f, 0.0f);
            mLook = new Vector3(0.0f, 0.0f, 0.0f);
        }
        public void setCameraType(CameraType cameraType)
        {
            mCameraType = cameraType;
        }
        //前后移动
        public void walk(float units)
        {
            // move only on xz plane for land object
            if (mCameraType == CameraType.LANDOBJECT)
                mPosition += new Vector3(mLook.X, 0.0f, mLook.Z) * units;

            if (mCameraType == CameraType.AIRCRAFT)
                mPosition += mLook * units;
        }
        //左右移动，扫射
        public void strafe(float units)
        {
            // move only on xz plane for land object
            if (mCameraType == CameraType.LANDOBJECT)
                mPosition += new Vector3(mRight.X, 0.0f, mRight.Z) * units;

            if (mCameraType == CameraType.AIRCRAFT)
                mPosition += mRight * units;
        }
        //上下移动
        public void fly(float units)
        {
            // move only on y-axis for land object
            if (mCameraType == CameraType.LANDOBJECT)
                mPosition.Y += units;

            if (mCameraType == CameraType.AIRCRAFT)
                mPosition += mUp * units;
        }

        // 倾斜角,上下摇动Camera
        public void Pitch(float angle)
        {
            Matrix T = Matrix.Identity;
            //D3DXMatrixRotationAxis(&T, &_right, angle);
            T.RotateAxis(mRight, angle);
            // rotate _up and _look around _right vector
            mUp.TransformCoordinate(T);
            mLook.TransformCoordinate(T);
            //D3DXVec3TransformCoord(&_up, &_up, &T);
            //D3DXVec3TransformCoord(&_look, &_look, &T);
        }
        //俯仰角,绕视线旋转Camera
        public void Roll(float angle)
        {
            // only roll for aircraft type
            if (mCameraType == CameraType.AIRCRAFT)
            {
                Matrix T = Matrix.Identity;
                T.RotateAxis(mLook, angle);

                // rotate _up and _right around _look vector
                mRight.TransformCoordinate(T);
                mUp.TransformCoordinate(T);
            }
        }
        // 航偏角,左右摇动Camera
        public void Yaw(float angle)
        {
            Matrix T = Matrix.Identity;

            // rotate around world y (0, 1, 0) always for land object
            if (mCameraType == CameraType.LANDOBJECT)
                T.RotateY(angle);
            // rotate around own up vector for aircraft
            if (mCameraType == CameraType.AIRCRAFT)
                T.RotateAxis(mUp, angle);

            // rotate _right and _look around _up or y-axis
            mRight.TransformCoordinate(T);
            mLook.TransformCoordinate(T);
        }
        //
        public void SetPosition(Vector3 position)// 设置相机世界坐标  
        {
            mPosition = position;
        }
        //
        public void RotateRay(float angle, Vector3 vOrigin, Vector3 vAxis)
        {
            // 计算新的焦点
            Vector3 vView = mLook - vOrigin;
            Matrix temp = Matrix.RotationAxis(vAxis, angle);
            vView.TransformCoordinate(temp);
            //vView.RotateAxis(angle, vAxis);
            mLook = vOrigin + vView;

            // 计算新的视点
            vView = mPosition - vOrigin;
           // Matrix temp2 = Matrix.RotationAxis(vAxis, angle);
            vView.TransformCoordinate(temp);
            //vView.RotateAxis(angle, vAxis);
            mPosition = vOrigin + vView;

            mUp.TransformCoordinate(temp);
          //  m_strafe.RotateAxis(angle, vAxis);
        }
        //更新相机状态
        public Matrix UpdateCamera()
        {
            Matrix mViewMatrix = Matrix.Identity;
            // Keep camera's axes orthogonal to eachother
            //D3DXVec3Normalize(&_look, &_look);
            mLook.Normalize();
            //D3DXVec3Cross(&, &_look, &_right);
            // _up = Vector3.Cross(_look, _right);
            //D3DXVec3Normalize(&_up, &_up);
            mUp.Normalize();
            //D3DXVec3Cross(&_right, &_up, &_look);
            mRight = Vector3.Cross(mUp, mLook);
            //D3DXVec3Normalize(&_right, &_right);
            mRight.Normalize();
            // Build the view matrix:
            //float x = -D3DXVec3Dot(&_right, &_pos);
            //float y = -D3DXVec3Dot(&_up, &_pos);
            //float z = -D3DXVec3Dot(&_look, &_pos);
            float x = -Vector3.Dot(mRight, mPosition);
            float y = -Vector3.Dot(mUp, mPosition);
            float z = -Vector3.Dot(mLook, mPosition);

            mViewMatrix.M11 = mRight.X; mViewMatrix.M12 = mUp.X; mViewMatrix.M13 = mLook.X; mViewMatrix.M14 = 0.0f;
            mViewMatrix.M21 = mRight.Y; mViewMatrix.M22 = mUp.Y; mViewMatrix.M23 = mLook.Y; mViewMatrix.M24 = 0.0f;
            mViewMatrix.M31 = mRight.Z; mViewMatrix.M32 = mUp.Z; mViewMatrix.M33 = mLook.Z; mViewMatrix.M34 = 0.0f;
            mViewMatrix.M41 = x; mViewMatrix.M42 = y; mViewMatrix.M43 = z; mViewMatrix.M44 = 1.0f;
            return mViewMatrix;
        }

        public void Update(Microsoft.DirectX.Direct3D.Device m_Device3d)
        {
            Matrix V = UpdateCamera();
            m_Device3d.SetTransform(TransformType.View, V);
        }
        //视口大小
        public Viewport Viewport
        {
            get
            {
                return mViewPort;
            }
        }
        //观察变换矩阵
        public Matrix ViewMatrix
        {
            get
            {
                return m_ViewMatrix;
            }
        }
        //投影变换矩阵
        public Matrix ProjectionMatrix
        {
            get
            {
                return m_ProjectionMatrix;
            }
        }
        //世界变换矩阵
        public Matrix WorldMatrix
        {
            get
            {
                return m_WorldMatrix;
            }
        }
        /// <summary>
        /// UnProject和Project之前需要调用该方法
        /// </summary>
        /// <param name="m_Device3d"></param>
        public void ComputeMatrix(Device m_Device3d)
        {
            m_WorldMatrix = m_Device3d.GetTransform(TransformType.World);
            m_ProjectionMatrix = m_Device3d.GetTransform(TransformType.Projection);
            m_ViewMatrix = m_Device3d.GetTransform(TransformType.View);
            mViewPort = m_Device3d.Viewport;
        }
        /// <summary>
        /// Projects a point from world to screen coordinates.
        /// 计算指定世界坐标的屏幕坐标
        /// </summary>
        /// <param name="point">Point in world space</param>
        /// <returns>Point in screen space</returns>
        public Vector3 Project(Vector3 point)
        {
            point.Project(mViewPort, m_ProjectionMatrix, m_ViewMatrix, m_WorldMatrix);
            return point;
        }

        internal Vector3 UnProject(Vector3 v1)
        {
            v1.Unproject(mViewPort, m_ProjectionMatrix, m_ViewMatrix, m_WorldMatrix);
            return v1;
        }
    }
}
