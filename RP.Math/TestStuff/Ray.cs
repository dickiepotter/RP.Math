namespace RPUtil.Math.Math3D
{
    public class Ray
    {
        #region Properties

        /// <summary>
        /// Orientation
        /// </summary>
        public Vector Origion           { get; set;}

        /// <summary>
        /// Some point which the line passes through
        /// </summary>
        public Vector PassThroughPoint  {get; set;}

        #endregion

        #region Constructors

        public Ray(Vector orientation, Vector passThroughPoint)
        {
            Origion = orientation;
            PassThroughPoint = passThroughPoint;
        }

        #endregion
    }
}
