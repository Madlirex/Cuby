using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Vector4 = System.Numerics.Vector4;


namespace MadEngine;

public class Game : GameWindow
{
    private GameObject[] _scene;
    private Shader _litShader;
    private Shader _unlitShader;
    private Camera _camera = new();
    private GameObject _cube;
    private GameObject _shadowCube;

    public static Vector4 LightColor;
    public static Vector3 LightPos;
    
    private Vector2 _lastMousePos;
    private bool _isDraggingWindow;
    private bool _isRotatingCube;
    
    private Vector2 _viewportPosition = Vector2.Zero;
    private Vector2 _rotVelocity = Vector2.Zero;
    private Vector2 _posVelocity = Vector2.Zero;
    
    private unsafe Cursor* _handCursor;
    private unsafe Cursor* _grabCursor;
    private bool _wasHoveringObject;
    
    public unsafe Game(string title) : base(new GameWindowSettings()
    {
        UpdateFrequency = 60
    },
        new NativeWindowSettings()
    {
        WindowState = WindowState.Maximized, 
        Title = title,
        WindowBorder = WindowBorder.Hidden,
        TransparentFramebuffer = true,   
        StartVisible = true,
    })
    {
        GLFW.SetWindowAttrib(WindowPtr, WindowAttribute.Floating, true);
        _litShader = new Shader("Shaders/shader.vert", "Shaders/lit.frag");
        _unlitShader = new Shader("Shaders/shader.vert", "Shaders/unlit.frag");

        CursorState = CursorState.Normal;
        
        _handCursor = GLFW.CreateStandardCursor(CursorShape.PointingHand);
        _grabCursor = GLFW.CreateStandardCursor(CursorShape.ResizeAll);
        
        float[] vertices = {
            -0.5f, -0.5f, -0.5f,  0.0f, 0.0f, 0.0f, 0.0f, -1.0f,
            0.5f, -0.5f, -0.5f,  1.0f, 0.0f, 0.0f, 0.0f, -1.0f,
            0.5f,  0.5f, -0.5f,  1.0f, 1.0f, 0.0f, 0.0f, -1.0f,
            0.5f,  0.5f, -0.5f,  1.0f, 1.0f, 0.0f, 0.0f, -1.0f,
            -0.5f,  0.5f, -0.5f,  0.0f, 1.0f, 0.0f, 0.0f, -1.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, 0.0f, 0.0f, 0.0f, -1.0f,

            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f, 0.0f, 0.0f, 1.0f,
            0.5f, -0.5f,  0.5f,  1.0f, 0.0f, 0.0f, 0.0f, 1.0f,
            0.5f,  0.5f,  0.5f,  1.0f, 1.0f, 0.0f, 0.0f, 1.0f,
            0.5f,  0.5f,  0.5f,  1.0f, 1.0f, 0.0f, 0.0f, 1.0f,
            -0.5f,  0.5f,  0.5f,  0.0f, 1.0f, 0.0f, 0.0f, 1.0f,
            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f, 0.0f, 0.0f, 1.0f,

            -0.5f,  0.5f,  0.5f,  1.0f, 0.0f, -1.0f, 0.0f, 0.0f,
            -0.5f,  0.5f, -0.5f,  1.0f, 1.0f, -1.0f, 0.0f, 0.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f, -1.0f, 0.0f, 0.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f, -1.0f, 0.0f, 0.0f,
            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f, -1.0f, 0.0f, 0.0f,
            -0.5f,  0.5f,  0.5f,  1.0f, 0.0f, -1.0f, 0.0f, 0.0f,

            0.5f,  0.5f,  0.5f,  1.0f, 0.0f, 1.0f, 0.0f, 0.0f,
            0.5f,  0.5f, -0.5f,  1.0f, 1.0f, 1.0f, 0.0f, 0.0f,
            0.5f, -0.5f, -0.5f,  0.0f, 1.0f, 1.0f, 0.0f, 0.0f,
            0.5f, -0.5f, -0.5f,  0.0f, 1.0f, 1.0f, 0.0f, 0.0f,
            0.5f, -0.5f,  0.5f,  0.0f, 0.0f, 1.0f, 0.0f, 0.0f,
            0.5f,  0.5f,  0.5f,  1.0f, 0.0f, 1.0f, 0.0f, 0.0f,

            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f, 0.0f, -1.0f, 0.0f,
            0.5f, -0.5f, -0.5f,  1.0f, 1.0f, 0.0f, -1.0f, 0.0f,
            0.5f, -0.5f,  0.5f,  1.0f, 0.0f, 0.0f, -1.0f, 0.0f,
            0.5f, -0.5f,  0.5f,  1.0f, 0.0f, 0.0f, -1.0f, 0.0f,
            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f, 0.0f, -1.0f, 0.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f, 0.0f, -1.0f, 0.0f,

            -0.5f,  0.5f, -0.5f,  0.0f, 1.0f, 0.0f, 1.0f, 0.0f,
            0.5f,  0.5f, -0.5f,  1.0f, 1.0f, 0.0f, 1.0f, 0.0f,
            0.5f,  0.5f,  0.5f,  1.0f, 0.0f, 0.0f, 1.0f, 0.0f,
            0.5f,  0.5f,  0.5f,  1.0f, 0.0f, 0.0f, 1.0f, 0.0f,
            -0.5f,  0.5f,  0.5f,  0.0f, 0.0f, 0.0f, 1.0f, 0.0f,
            -0.5f,  0.5f, -0.5f,  0.0f, 1.0f, 0.0f, 1.0f, 0.0f
        };
        
        uint[] indices =
        {
            // Front (-Z)
            0, 2, 1,
            3, 5, 4,

            // Back (+Z)
            6, 7, 8,
            9, 10, 11,

            // Left (-X)
            12,13,14,
            15,16,17,

            // Right (+X)
            18,20,19,
            21,23,22,

            // Bottom (-Y)
            24,25,26,
            27,28,29,

            // Top (+Y)
            30,32,31,
            33,35,34
        };
        
        Mesh mesh = new Mesh(vertices, indices);
        Material material = new Material(_unlitShader, new Texture(SettingsManager.Settings.GetRandomTexture()), Vector4.One);
        
        MeshRenderer meshRenderer = new MeshRenderer(mesh, material);

        Transform transform = new Transform()
        {
            Position = new Vector3(0f, 0f, -2f),
            Rotation = new Vector3(33f, 33f, 0f)
        };
        Transform lightTransform = new Transform()
        {
            Position = new Vector3(2f, 2f, 0f)
        };
        Transform shadowTransform = new Transform()
        {
            Position = transform.Position + new Vector3(0.06f, -0.06f, -0.02f),
            Rotation = transform.Rotation,
            Scale = transform.Scale * 1.04f
        };

        Material shadowMaterial = new Material(_unlitShader, null, new Vector4(0f, 0f, 0f, 0.5f));
        _shadowCube = new GameObject(new MeshRenderer(mesh, shadowMaterial), shadowTransform);
        
        LightPos = lightTransform.Position;
        LightColor = new Vector4(1f, 1f, 1f, 1f);
        _cube = new GameObject(meshRenderer, transform);
        
        Mesh emptyMesh = new Mesh([], []);
        
        MeshRenderer lampRenderer =
            new MeshRenderer(emptyMesh, new Material(_unlitShader, null, new Vector4(1f, 1f, 1f, 1f)));
        
        GameObject lightObject = new GameObject(lampRenderer, lightTransform);
        _litShader.SetVector3("lightPos", lightObject.Transform.Position);
        
        _scene = [_cube, lightObject, _shadowCube];
        GL.Enable(EnableCap.DepthTest);
        
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        
        _camera.Width = FramebufferSize.X;
        _camera.Height = FramebufferSize.Y;

        foreach (GameObject gameObject in _scene)
        {
            gameObject.MeshRenderer.Mesh.Initialize();
        }
        
        int x = (int)_viewportPosition.X;
        int y = (int)_viewportPosition.Y;
        GL.Viewport(x, y, FramebufferSize.X, FramebufferSize.Y);
        
        GL.ClearColor(0f, 0f, 0f, 0f);
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        
        _camera.Width = FramebufferSize.X;
        _camera.Height = FramebufferSize.Y;
        int x = (int)_viewportPosition.X;
        int y = (int)_viewportPosition.Y;
        GL.Viewport(x, y, FramebufferSize.X, FramebufferSize.Y);
    }

    protected override unsafe void OnUnload()
    {
        base.OnUnload();
        SettingsManager.Save(SettingsManager.Settings);

        CursorState = CursorState.Normal;
        
        _litShader.Dispose();
        _unlitShader.Dispose();
        
        GLFW.DestroyCursor(_handCursor);
        GLFW.DestroyCursor(_grabCursor);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        _shadowCube.Transform.Position = _cube.Transform.Position + new Vector3(0.06f, -0.06f, -0.02f);
        _shadowCube.Transform.Rotation = _cube.Transform.Rotation;
        
        _litShader.SetVector3("viewPos", _camera.Transform.Position);
        _litShader.SetVector3("lightPos", LightPos);

        Matrix4 view = _camera.GetViewMatrix();
        Matrix4 projection = _camera.GetPerspectiveMatrix();

        GL.DepthMask(false);
        GL.Disable(EnableCap.DepthTest);

        _shadowCube.MeshRenderer.Draw(view, projection);

        GL.DepthMask(true);
        GL.Enable(EnableCap.DepthTest);

        _cube.MeshRenderer.Draw(view, projection);
        
        MouseState mouse = MouseState;

        float scaleX = FramebufferSize.X / (float)ClientSize.X;
        float scaleY = FramebufferSize.Y / (float)ClientSize.Y;

        int mouseX = (int)(mouse.X * scaleX);
        int mouseY = FramebufferSize.Y - (int)(mouse.Y * scaleY);

        bool inBounds = mouseX >= 0 && mouseX < FramebufferSize.X && mouseY >= 0 && mouseY < FramebufferSize.Y;
        
        byte[] pixelRgba = new byte[4];
        if (inBounds)
        {
            GL.ReadPixels(mouseX, mouseY, 1, 1, PixelFormat.Rgba, PixelType.UnsignedByte, pixelRgba);
        }
        byte pixelAlpha0 = pixelRgba[3];

        bool isHoveringObject = pixelAlpha0 > 0;
        unsafe
        {
            bool shouldAcceptInput = isHoveringObject || _isDraggingWindow || _isRotatingCube;
            
            GLFW.SetWindowAttrib(WindowPtr, WindowAttribute.MousePassthrough, !shouldAcceptInput);
            
            if (_isDraggingWindow || _isRotatingCube)
            {
                GLFW.SetCursor(WindowPtr, _grabCursor);
            }
            else if (isHoveringObject)
            {
                GLFW.SetCursor(WindowPtr, _handCursor);
            }
            else if (_wasHoveringObject)
            {
                GLFW.SetCursor(WindowPtr, null);
            }
            _wasHoveringObject = isHoveringObject;
        }

        SwapBuffers();
    }

   protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        if (KeyboardState.IsKeyDown(Keys.Escape))
        {
            Close();
        }

        MouseState mouse = MouseState;
        Vector2 currentMousePos = new Vector2(mouse.X, mouse.Y);
        
        if (mouse.IsButtonDown(MouseButton.Left))
        {
            if (!_isDraggingWindow)
            {
                _isDraggingWindow = true;
                _lastMousePos = currentMousePos;
                _posVelocity = Vector2.Zero;
            }
            else
            {
                Vector2 delta = currentMousePos - _lastMousePos;
                
                _posVelocity = delta * SettingsManager.Settings.DragSensitivity;

                ApplyCubeMovement(_posVelocity.X, _posVelocity.Y);
                _lastMousePos = currentMousePos;
            }
        }
        else
        {
            _isDraggingWindow = false;
            
            if (_posVelocity.LengthSquared > 0.00001f)
            {
                ApplyCubeMovement(_posVelocity.X, _posVelocity.Y);
                _posVelocity *= SettingsManager.Settings.Friction;
            }
        }
        
        if (mouse.IsButtonDown(MouseButton.Right))
        {
            if (!_isRotatingCube)
            {
                _isRotatingCube = true;
                _lastMousePos = currentMousePos;
                _rotVelocity = Vector2.Zero;
            }
            else
            {
                Vector2 delta = currentMousePos - _lastMousePos;
                _rotVelocity = delta * + SettingsManager.Settings.RotSensitivity;

                _cube.Transform.Rotation.Y += _rotVelocity.X;
                _cube.Transform.Rotation.X += _rotVelocity.Y;

                _lastMousePos = currentMousePos;
            }
        }
        else
        {
            _isRotatingCube = false;
            
            if (_rotVelocity.LengthSquared > 0.0001f)
            {
                _cube.Transform.Rotation.Y += _rotVelocity.X;
                _cube.Transform.Rotation.X += _rotVelocity.Y;
                _rotVelocity *= SettingsManager.Settings.Friction;
            }
        }
        
        BounceCubeOffScreen();
    }
   
    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);
        
        float zoomDelta = -e.OffsetY * SettingsManager.Settings.ZoomSensitivity; 

        _camera.Fov -= zoomDelta;
    }
   
    private void ApplyCubeMovement(float deltaX, float deltaY)
    {
        _viewportPosition.X += deltaX;
        _viewportPosition.Y -= deltaY;
        
        int x = (int)_viewportPosition.X;
        int y = (int)_viewportPosition.Y;
        GL.Viewport(x, y, FramebufferSize.X, FramebufferSize.Y);
    }
    
    private void BounceCubeOffScreen()
    {
        Vector3[] corners =
        {
            new(-0.5f,-0.5f,-0.5f),
            new( 0.5f,-0.5f,-0.5f),
            new(-0.5f, 0.5f,-0.5f),
            new( 0.5f, 0.5f,-0.5f),

            new(-0.5f,-0.5f, 0.5f),
            new( 0.5f,-0.5f, 0.5f),
            new(-0.5f, 0.5f, 0.5f),
            new( 0.5f, 0.5f, 0.5f)
        };

        Matrix4 model = _cube.Transform.GetModuleMatrix();
        Matrix4 view = _camera.GetViewMatrix();
        Matrix4 projection = _camera.GetPerspectiveMatrix();

        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;

        foreach (var corner in corners)
        {
            OpenTK.Mathematics.Vector4 p = new OpenTK.Mathematics.Vector4(corner.X, corner.Y, corner.Z, 1.0f);

            p = OpenTK.Mathematics.Vector4.TransformRow(p, model);
            p = OpenTK.Mathematics.Vector4.TransformRow(p, view);
            p = OpenTK.Mathematics.Vector4.TransformRow(p, projection);

            p /= p.W;

            float screenX = (p.X * 0.5f + 0.5f) * FramebufferSize.X + _viewportPosition.X;
            float screenY = (1.0f - (p.Y * 0.5f + 0.5f)) * FramebufferSize.Y - _viewportPosition.Y;

            minX = MathF.Min(minX, screenX);
            maxX = MathF.Max(maxX, screenX);

            minY = MathF.Min(minY, screenY);
            maxY = MathF.Max(maxY, screenY);
        }

        if (minX < 0)
        {
            _viewportPosition.X -= minX;
            _posVelocity.X = MathF.Abs(_posVelocity.X) * SettingsManager.Settings.Restitution;
        }

        if (maxX > FramebufferSize.X)
        {
            _viewportPosition.X -= maxX - FramebufferSize.X;
            _posVelocity.X = -MathF.Abs(_posVelocity.X) * SettingsManager.Settings.Restitution;
        }

        if (minY < 0)
        {
            _viewportPosition.Y += minY;
            _posVelocity.Y = MathF.Abs(_posVelocity.Y) * SettingsManager.Settings.Restitution;
        }

        if (maxY > FramebufferSize.Y)
        {
            _viewportPosition.Y += maxY - FramebufferSize.Y;
            _posVelocity.Y = -MathF.Abs(_posVelocity.Y) * SettingsManager.Settings.Restitution;
        }

        GL.Viewport((int)_viewportPosition.X, (int)_viewportPosition.Y, FramebufferSize.X, FramebufferSize.Y);
    }
}