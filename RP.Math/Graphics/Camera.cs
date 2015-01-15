using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Math.Graphics
{
    public class Camera
    {
        public double ViewportWidth {get; set; }
        public double ViewportHeight {get; set; }
        public float Near {get; set; }
        public float Far {get; set; }

        
    }
}

/*
using System;
using System.Diagnostics;
using System.Collections;
using System.Drawing;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Xml;
using Exocortex;
using Exocortex.Collections;
using Exocortex.Geometry3D;
using Exocortex.OpenGL;

namespace ExoEngine.Geometry {

	public class Camera {

		//---------------------------------------------------------------------------------

		public Camera() {
			this.SetCamera( new Matrix3() );
			this.SetClipDistances( 10, 1000 );
			this.SetViewport( 5, 5 );
		}
		
		//---------------------------------------------------------------------------------

		protected Matrix3	_xfrm;
		protected Matrix3	_xfrmInverse;
		protected Vector3		_translation;		
		protected Vector3		_forwardAxis;	// forward = + z
		protected Vector3		_upAxis;		// up = + y
		protected Vector3		_rightAxis;		// right = - x

		public Matrix3	Transform {
			get {	return	_xfrm;	}
		}
		public Matrix3	TransformInverse {
			get {	return	_xfrmInverse;	}
		}
		public Vector3		Translation {
			get {	return	_translation;	}
		}
		public Vector3		ForwardAxis {
			get {	return	_forwardAxis;	}
		}
		public Vector3		UpAxis {
			get {	return	_upAxis;	}
		}
		public Vector3		RightAxis {
			get {	return	_rightAxis;	}
		}

		public	void	SetCamera( Matrix3 xfrm ) {
			_xfrmInverse	= xfrm;
			_xfrm			= _xfrmInverse.GetInverse();
			_translation	= _xfrmInverse.ExtractTranslation();
			xfrm.ExtractBasis(
				out _rightAxis,		// x axis
				out _upAxis,		// y axis
				out _forwardAxis	// z axis
				);
			_rightAxis = -_rightAxis;
			_bFrustumPlanesDirty = true;
		}

		//---------------------------------------------------------------------------------

		protected float	_viewportWidth;
		protected float	_viewportHeight;

		public float	ViewportWidth {
			get {	return	_viewportWidth;	}
		}
		public float	ViewportHeight {
			get {	return	_viewportHeight;	}
		}

		public void	SetViewport( float viewportWidth, float viewportHeight ) {
			_viewportWidth	= viewportWidth;
			_viewportHeight	= viewportHeight;
			_bFrustumPlanesDirty = true;
		}

		//---------------------------------------------------------------------------------

		protected float	_nearClipDistance;
		protected float	_farClipDistance;

		public float	NearClipDistance {
			get {	return	_nearClipDistance;	}
		}
		public float	FarClipDistance {
			get {	return	_farClipDistance;	}
		}

		public void	SetClipDistances( float nearClipDistance, float farClipDistance ) {
			_nearClipDistance	= nearClipDistance;
			_farClipDistance	= farClipDistance;
			_bFrustumPlanesDirty = true;
		}

		//---------------------------------------------------------------------------------

		protected bool	_bFrustumPlanesDirty = true;

		protected Plane3	_nearPlane;
		protected Plane3	_farPlane;
		protected Plane3	_topPlane;
		protected Plane3	_bottomPlane;
		protected Plane3	_rightPlane;
		protected Plane3	_leftPlane;

		public Plane3	NearPlane {
			get {
				SyncFrustumPlanes();
				return	_nearPlane;
			}
		}
		public Plane3	FarPlane {
			get {
				SyncFrustumPlanes();
				return	_farPlane;
			}
		}
		public Plane3	TopPlane {
			get {
				SyncFrustumPlanes();
				return	_topPlane;
			}
		}
		public Plane3	BottomPlane {
			get {
				SyncFrustumPlanes();
				return	_bottomPlane;
			}
		}
		public Plane3	RightPlane {
			get {
				SyncFrustumPlanes();
				return	_rightPlane;
			}
		}
		public Plane3	LeftPlane {
			get {
				SyncFrustumPlanes();
				return	_leftPlane;
			}
		}

		protected void	SyncFrustumPlanes() {
			if( _bFrustumPlanesDirty == false ) {
				return;
			}

			Vector3 nearClipTranslation	= _translation + _forwardAxis * _nearClipDistance;
			Vector3 farClipTranslation	= _translation + _forwardAxis * _nearClipDistance;

			_topPlane = Plane3.FromNormalAndPoint( _forwardAxis, _translation + nearClipTranslation );
			_farPlane = Plane3.FromNormalAndPoint( _forwardAxis, _translation + farClipTranslation );

			Vector3 viewportUpTranslation		= _upAxis * _viewportHeight / 2; 
			Vector3 viewportRightTranslation	= _rightAxis * _viewportWidth / 2; 

			Vector3 topNormal = Vector3.Cross( _rightAxis, nearClipTranslation + viewportUpTranslation ).GetUnit();
			_topPlane = Plane3.FromNormalAndPoint( topNormal, _translation + farClipTranslation );

			Vector3 bottomNormal = Vector3.Cross( - _rightAxis, nearClipTranslation - viewportUpTranslation ).GetUnit();
			_bottomPlane = Plane3.FromNormalAndPoint( bottomNormal, _translation + farClipTranslation );

			Vector3 rightNormal = Vector3.Cross( - _upAxis, nearClipTranslation + viewportRightTranslation ).GetUnit();
			_rightPlane = Plane3.FromNormalAndPoint( rightNormal, _translation + farClipTranslation );

			Vector3 leftNormal = Vector3.Cross( _upAxis, nearClipTranslation - viewportRightTranslation ).GetUnit();
			_leftPlane = Plane3.FromNormalAndPoint( leftNormal, _translation + farClipTranslation );

			_bFrustumPlanesDirty = false;
		}

		//---------------------------------------------------------------------------------

	}
}
*/

/*

namespace Jade.Core
{
	/// <summary>
	/// Basic camera class - this contains just the information required
	/// for doing a simple rendering
	/// </summary>
	public class Camera
	{
		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="Camera"/> class.
		/// </summary>
		public Camera()
		{
			_transform = new OrientationPosition();
		}
		#endregion

		ITransform _transform;
		/// <summary>
		/// Gets or sets the transform associated with this camera.
		/// </summary>
		/// <value>The transform.</value>
		public ITransform Transform
		{
			get { return _transform; }
			set { _transform = value; }
		}

		private float _fieldOfView = 45f;
		/// <summary>
		/// Gets or sets the field of view.
		/// </summary>
		/// <value>The field of view.</value>
		public float FieldOfView
		{
			get { return _fieldOfView; }
			set { _fieldOfView = value; }
		}

		private float _aspectRatio = 1f;
		/// <summary>
		/// Gets or sets the aspect ratio for this camera.
		/// </summary>
		/// <value>The aspect ratio.</value>
		public float AspectRatio
		{
			get { return _aspectRatio; }
			set { _aspectRatio = value; }
		}

		private float _nearPlane = 0.1f;
		/// <summary>
		/// Gets or sets the near plane.
		/// </summary>
		/// <value>The near plane.</value>
		public float NearPlane
		{
			get { return _nearPlane; }
			set { _nearPlane = value; }
		}

		private float _farPlane = 1000f;
		/// <summary>
		/// Gets or sets the far plane.
		/// </summary>
		/// <value>The far plane.</value>
		public float FarPlane
		{
			get { return _farPlane; }
			set { _farPlane = value; }
		}

		#region Methods

		/// <summary>
		/// Creates a render list for this camera
		/// </summary>
		public RenderList CreateRenderList(Node root, Node overlay)
		{
			RenderList renderList = new RenderList();
			if (root != null)
			{
				Vector3d pos = this.Transform.Position * -1.0; // Make a matrix to undo our translation
				root.Walk(new CullNodeWalker(renderList), Matrixd.CreateTranslation(ref pos));
			}

			if (overlay != null)
				overlay.Walk(new CullOverlayNodeWalker(renderList), Matrixd.Identity);

			return renderList;
		}

		#endregion
	}
}
*/

/*namespace BattleTank2005
{
	public class Camera
	{
		public Camera()
		{
			_viewMatrix = Matrix.Identity;
			_perspectiveMatrix = Matrix.PerspectiveFovLH ( _fieldOfView, _aspectRatio, _nearPlane, _farPlane );
		}

		public void MoveCameraLeftRight( float deltaHeading )
		{
            _heading += ConvertDegreesToRadians ( deltaHeading );

            if ( _heading > ( 2.0f * Math.PI ) )
			{
                _heading -= (float)( 2.0f * Math.PI );
			}

            if ( _heading < 0.0f )
			{
                _heading += (float)( 2.0f * Math.PI );
			}

		}
		public void MoveCameraUpDown( float deltaPitch )
		{
            _pitch += ConvertDegreesToRadians ( deltaPitch );

            if ( _pitch > ( 0.5f * Math.PI ) )
			{
                _pitch = (float)( 0.5f * Math.PI );
			}

            if ( _pitch < ( -0.5f * Math.PI ) )
			{
                _pitch = (float)( -0.5f * Math.PI );
			}

		}
        public void MoveCameraPosition ( float x, float y, float z )
        {
            _x += x * (float)Math.Cos ( _heading ) + z * (float)Math.Sin ( _heading );
            _y += y;
            _z += z * (float)Math.Cos ( _heading ) + x * (float)Math.Sin ( _heading );
        }
        public void Render ( Device device )
        {
            cameraPosition = new Vector3 ( _x, _y, _z );

            cameraTarget = cameraPosition;
            cameraTarget.X += (float)Math.Sin ( _heading ) * 10.0f;
            cameraTarget.Y += (float)Math.Sin ( _pitch ) * 10.0f;
            cameraTarget.Z += (float)Math.Cos ( _heading) * 10.0f;

            // Set the app view matrix for normal viewing
            _viewMatrix = Matrix.LookAtLH ( cameraPosition, cameraTarget, new Vector3 ( 0.0f, 1.0f, 0.0f ) );

            device.Transform.View = _viewMatrix;
            device.Transform.Projection = _perspectiveMatrix;
        }

        public float Heading 
        {
            get 
            { 
                return (float)( _heading * 180.0 / Math.PI ); 
            } 
        }
        public float Pitch 
        {
            get 
            { 
                return (float)( _pitch * 180.0 / Math.PI ); 
            } 
        }
        public Matrix View 
        { 
            get { return _viewMatrix; } 
        }

        public float X
        {
            get { return _x; }
        }
        public float Y
        {
            get { return _y; }
        }
        public float Z
        {
            get { return _z; }
        }

        private float ConvertDegreesToRadians ( float degree )
        {
            return degree * (float)( Math.PI / 180 );
        }

        private float _pitch;
        private float _heading;
        private float _x = 1.0f;
        private float _y = 1.0f;
        private float _z = 1.0f;
        private Matrix _viewMatrix;
        private Matrix _perspectiveMatrix;
        private Vector3 cameraPosition;
        private Vector3 cameraTarget;
        private float _fieldOfView = (float)Math.PI / 4.0f;
        private float _aspectRatio = 1.33f;
        private float _nearPlane = 1.0f;
        private float _farPlane = 800.0f;
	}
}*/