using System;
using System.Collections.Generic;
using System.Linq;
using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using Fusee.Serialization;
using Fusee.Xene;
using static System.Math;
using static Fusee.Engine.Core.Input;
using static Fusee.Engine.Core.Time;

namespace Fusee.Tutorial.Core
{
    public class AssetsPicking : RenderCanvas
    {
        private SceneContainer _scene;
        private SceneRenderer _sceneRenderer;
        private TransformComponent _schaufelTransform;
        private TransformComponent  _oberarmTransform;
        private TransformComponent  _unterarmTransform;
        private TransformComponent _leftFrontWheelTransform;
        private TransformComponent _rightFrontWheelTransform;
        private TransformComponent _leftRearWheelTransform;
        private TransformComponent _rightRearWheelTransform;
        private TransformComponent _bodyTransform;
        private ScenePicker _scenePicker;
         private PickResult _currentPick;
        private float3 _oldColor;
        private float _camAngle = 0;
       

        // Init is called on startup. 
        public override void Init()
        {
            // Set the clear color for the backbuffer to white (100% intentsity in all color channels R, G, B, A).
            RC.ClearColor = new float4(0.8f, 0.9f, 0.7f, 1);

            _scene = AssetStorage.Get<SceneContainer>("Traktor.fus");

            _bodyTransform = _scene.Children.FindNodes(Node => Node.Name == "Body")?.FirstOrDefault()?.GetTransform();

            _schaufelTransform = _scene.Children.FindNodes(Node => Node.Name == "Schaufel")?.FirstOrDefault()?.GetTransform();

            _oberarmTransform = _scene.Children.FindNodes(Node => Node.Name == "Oberarm")?.FirstOrDefault()?.GetTransform();

            _unterarmTransform = _scene.Children.FindNodes(Node => Node.Name == "Arm")?.FirstOrDefault()?.GetTransform();

            _leftFrontWheelTransform = _scene.Children.FindNodes(Node => Node.Name == "LeftFrontWheel")?.FirstOrDefault()?.GetTransform();
            _rightFrontWheelTransform = _scene.Children.FindNodes(Node => Node.Name == "RightFrontWheel")?.FirstOrDefault()?.GetTransform();
            _leftRearWheelTransform = _scene.Children.FindNodes(Node => Node.Name == "LeftRearWheel")?.FirstOrDefault()?.GetTransform();
            _rightRearWheelTransform = _scene.Children.FindNodes(Node => Node.Name == "RightRearWheel")?.FirstOrDefault()?.GetTransform();
           

            // Create a scene renderer holding the scene above
            _sceneRenderer = new SceneRenderer(_scene);
            _scenePicker = new ScenePicker(_scene);
        }

        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {
            // _baseTransform.Rotation = new float3(0, M.MinAngle(TimeSinceStart), 0);

            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

                      
            
            float speed = Keyboard.UpDownAxis * 15.0f;
            float rotSpeed = Keyboard.LeftRightAxis * 3.0f;
            float wheelRotSpeed = speed * Time.DeltaTime * 0.5f;
            float wheelRotSpeedDelta = rotSpeed * Time.DeltaTime * 0.5f;
            float yRot = _bodyTransform.Rotation.y + rotSpeed * Time.DeltaTime;
            float3 speed3d = new float3(speed * M.Sin(yRot), 0, speed * M.Cos(yRot));

            _bodyTransform.Translation = _bodyTransform.Translation + speed3d * Time.DeltaTime;
            _bodyTransform.Rotation = new float3(_bodyTransform.Rotation.x, yRot, _bodyTransform.Rotation.z);
            

            _rightRearWheelTransform.Rotation = new float3(_rightFrontWheelTransform.Rotation.x + wheelRotSpeed - wheelRotSpeedDelta, 0, 0);
            _leftRearWheelTransform.Rotation = new float3(_leftRearWheelTransform.Rotation.x + wheelRotSpeed + wheelRotSpeedDelta, 0, 0);
            _rightFrontWheelTransform.Rotation = new float3(_rightFrontWheelTransform.Rotation.x +wheelRotSpeed - wheelRotSpeedDelta, 0, 0);
            _leftFrontWheelTransform.Rotation = new float3(_leftFrontWheelTransform.Rotation.x + wheelRotSpeed + wheelRotSpeedDelta, 0, 0);
          

            // Setup the camera 
            RC.View = float4x4.CreateTranslation(0, -10, 50) * float4x4.CreateRotationY(_camAngle);
            
       

            
            if(Mouse.RightButton == true){
                _camAngle += Mouse.Velocity.x * 0.01f * DeltaTime;
            }

            if (Mouse.LeftButton)
            {
                float2 pickPosClip = Mouse.Position * new float2(2.0f / Width, -2.0f / Height) + new float2(-1, 1);
                
                _scenePicker.View = RC.View;
                _scenePicker.Projection = RC.Projection;
                
                List<PickResult> pickResults = _scenePicker.Pick(pickPosClip).ToList();
                
                PickResult newPick = null;
               
                if (pickResults.Count > 0)
                {
                    pickResults.Sort((a, b) => Sign(a.ClipPos.z - b.ClipPos.z));
                    newPick = pickResults[0];
                }
                if (newPick?.Node != _currentPick?.Node)
                {
                    if (_currentPick != null)
                    {
                        ShaderEffectComponent shaderEffectComponent = _currentPick.Node.GetComponent<ShaderEffectComponent>();
                        shaderEffectComponent.Effect.SetEffectParam("DiffuseColor", _oldColor);
                        
                    }
                    if (newPick != null)
                    {
                        ShaderEffectComponent shaderEffectComponent = newPick.Node.GetComponent<ShaderEffectComponent>();
                        _oldColor = (float3)shaderEffectComponent.Effect.GetEffectParam("DiffuseColor");
                        shaderEffectComponent.Effect.SetEffectParam("DiffuseColor", new float3(0.5f, 1, 0.4f));
                    }
                   
                    _currentPick = newPick;
                }

                
            }

            if(_currentPick?.Node.Name == "Schaufel"){
                float schaufelTransform = _schaufelTransform.Rotation.x;
                schaufelTransform -= 0.01f * Keyboard.WSAxis;
               
                if(schaufelTransform >= 0.8f){
                    schaufelTransform = 0.8f;
                }else if(schaufelTransform <= -1.0f){
                    schaufelTransform = -1.0f;
                }
                _schaufelTransform.Rotation = new float3(schaufelTransform, 0, 0);
            }else if(_currentPick?.Node.Name == "Oberarm"){
                float oberarmTransform = _oberarmTransform.Rotation.x;
                oberarmTransform -= 0.01f * Keyboard.WSAxis;
                
                if(oberarmTransform >= 1.0f){
                    oberarmTransform = 1.0f;
                }else if(oberarmTransform <= -0.5f){
                    oberarmTransform = -0.5f;
                }
                _oberarmTransform.Rotation = new float3(oberarmTransform, 0, 0);
            }else if(_currentPick?.Node.Name == "Arm"){
                float unterarmTransform = _unterarmTransform.Rotation.x;
                unterarmTransform -= 0.01f * Keyboard.WSAxis;
                 if(unterarmTransform >= 1.5f){
                    unterarmTransform = 1.5f;
                }else if(unterarmTransform <= -0.01f){
                    unterarmTransform = -0.01f;
                }
                _unterarmTransform.Rotation = new float3(unterarmTransform, 0, 0);
            } 

            // if(_currentPick?.Node.Name == "LeftFrontWheel" || _currentPick?.Node.Name == "RightFrontWheel" || _currentPick?.Node.Name == "LeftRearWheel" || _currentPick?.Node.Name == "RightRearWheel"){
            //     float leftFrontWheelTransform = _leftFrontWheelTransform.Rotation.x;
            //     leftFrontWheelTransform -= 0.08f * Keyboard.UpDownAxis;
            //     _leftFrontWheelTransform.Rotation = new float3(leftFrontWheelTransform, 0, 0);

            //     float rightFrontWheelTransform = _rightFrontWheelTransform.Rotation.x;
            //     rightFrontWheelTransform -= 0.08f * Keyboard.UpDownAxis;
            //     _rightFrontWheelTransform.Rotation = new float3(rightFrontWheelTransform, 0, 0);

            //     float leftRearWheelTransform = _leftRearWheelTransform.Rotation.x;
            //     leftRearWheelTransform -= 0.08f * Keyboard.UpDownAxis;
            //     _leftRearWheelTransform.Rotation = new float3(leftRearWheelTransform, 0, 0);

            //     float rightRearWheelTransform = _rightRearWheelTransform.Rotation.x;
            //     rightRearWheelTransform -= 0.08f * Keyboard.UpDownAxis;
            //     _rightRearWheelTransform.Rotation = new float3(rightRearWheelTransform, 0, 0);
            // }

            // Render the scene on the current render context
            _sceneRenderer.Render(RC);

            // Swap buffers: Show the contents of the backbuffer (containing the currently rendered farame) on the front buffer.
            Present();
        }


        // Is called when the window was resized
        public override void Resize()
        {
            // Set the new rendering area to the entire new windows size
            RC.Viewport(0, 0, Width, Height);

            // Create a new projection matrix generating undistorted images on the new aspect ratio.
            var aspectRatio = Width / (float)Height;

            // 0.25*PI Rad -> 45� Opening angle along the vertical direction. Horizontal opening angle is calculated based on the aspect ratio
            // Front clipping happens at 1 (Objects nearer than 1 world unit get clipped)
            // Back clipping happens at 2000 (Anything further away from the camera than 2000 world units gets clipped, polygons will be cut)
            var projection = float4x4.CreatePerspectiveFieldOfView(M.PiOver4, aspectRatio, 1, 20000);
            RC.Projection = projection;
        }
    }
}
