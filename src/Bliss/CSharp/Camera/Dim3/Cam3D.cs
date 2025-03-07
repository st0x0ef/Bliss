using System.Numerics;
using Bliss.CSharp.Windowing;
using Vortice.Mathematics;
using Viewport = Veldrid.Viewport;

namespace Bliss.CSharp.Camera.Dim3;

public class Cam3D : Disposable, ICam {
    
    public Window Window { get; private set; }
    public Viewport Viewport { get; private set; }
    
    public float AspectRatio { get; private set; }
    public float NearPlane { get; private set; }
    public float FarPlane { get; private set; }

    public Vector3 Position;
    public Vector3 Target;
    public Vector3 Up;
    public ProjectionType ProjectionType;
    public float Fov;
    
    private Vector3 _rotation;
    
    public Cam3D(Window window, Vector3 position, Vector3 target, Vector3? up = default, ProjectionType projectionType = ProjectionType.Perspective, float fov = 70.0F) {
        this.Window = window;
        this.Window.Resized += this.Resize;

        this.Position = position;
        this.Target = target;
        this.Up = up ?? -Vector3.UnitY; // TODO CHECK IF negative is right.
        this.ProjectionType = projectionType;
        this.Fov = fov;
        
        this.NearPlane = 0.001F;
        this.FarPlane = 1000.0F;
        
        this.Resize();
    }

    public Vector3 GetForward() {
        return this.Target - this.Position;
    }

    public Vector3 GetRight() {
        return Vector3.Cross(this.GetForward(), this.Up);
    }

    public Vector3 GetRotation() {
        return this._rotation;
    }

    public void SetYaw(float angle, bool rotateAroundTarget) {
        Vector3 direction = Vector3.Normalize(this.Position - this.Target);
        float distance = Vector3.Distance(this.Position, this.Target);
            
        // Calculate the new position
        Vector3 targetPos = this.Target + Vector3.Transform(direction * distance, Matrix4x4.CreateRotationY(MathF.PI * angle / 180f));
        
        if (rotateAroundTarget) {
            this.Position = targetPos;
        }
        else {
            this.Target = targetPos;
        }
    }
    
    public Matrix4x4 GetProjection() {
        if (this.ProjectionType == ProjectionType.Perspective) {
            return Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.ToRadians(this.Fov), this.AspectRatio, this.NearPlane, this.FarPlane);
        }
        
        return Matrix4x4.CreateOrthographicOffCenter(-40, 40, 40, -40, this.NearPlane, this.FarPlane);
    }

    public Matrix4x4 GetView() {
        return Matrix4x4.CreateLookAt(this.Position, this.Target, this.Up);
    }

    private void Resize() {
        this.Viewport = new Viewport(0, 0, this.Window.Width, this.Window.Height, 0, 0);
        this.AspectRatio = (float) this.Window.Width / (float) this.Window.Height;
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            this.Window.Resized -= this.Resize;
        }
    }
} 