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
    public class FirstSteps : RenderCanvas
    {

        private SceneContainer _scene;
        private SceneRenderer _sceneRenderer;
        private float _camAngle = 0;
        private TransformComponent _cubeTransform;
        private float gCi = 0.5f; 
   

        // Init is called on startup. 
        public override void Init(){
            // Set the clear color for the backbuffer to light green (intensities in R, G, B, A).
            //RC.ClearColor = new float4(0.5f, 0.8f, 0.7f, 1);

            RC.ClearColor = ColorUint.Tofloat4(ColorUint.Black);

            //creating scene with object
            _scene = new SceneContainer();
            _scene.Children = new List<SceneNodeContainer>();
            
            addCube(7);

            //render scene above
            _sceneRenderer = new SceneRenderer(_scene);

        }


        private Random random = new Random();
        public void addCube(int amount){
             
            for(int i = 0; i < amount; i++){
                _cubeTransform = new TransformComponent { Scale = new float3(1, 1, 1), Translation = new float3( (float) random.Next( -50, 50), (float) random.Next( -50, 50), (float) random.Next( -50, 50))};
                var cubeShader  = new ShaderEffectComponent {
                    Effect = SimpleMeshes.MakeShaderEffect(new float3((float) 1/(i+1), (float) 1/(i+1),(float) 1/(i+1)), new float3(1, 1, 1), 4)
                };
            
                var cubeMesh = SimpleMeshes.CreateCuboid(new float3(random.Next( 5, 20), random.Next( 5, 20), random.Next( 5, 20)));
                
                //assemble cube node 
                var cubeNode = new SceneNodeContainer();
                cubeNode.Components = new List<SceneComponentContainer>();
                cubeNode.Components.Add(_cubeTransform);
                cubeNode.Components.Add(cubeShader);
                cubeNode.Components.Add(cubeMesh);

                _scene.Children.Add(cubeNode);
            }
        }
       

        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {
     
           _camAngle = _camAngle + 90.0f * M.Pi/180.0f *DeltaTime;
            
            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);
            
            //EPILEPSIE-GEFAHR! 
            RC.ClearColor = new float4(RC.ClearColor.r, RC.ClearColor.g + gCi, RC.ClearColor.b, RC.ClearColor.a);
            if(RC.ClearColor.g >= 1 || RC.ClearColor.g >= 0)
            gCi = -gCi; 

            //camera postition
           RC.View = float4x4.CreateTranslation(0, 0, 200) * float4x4.CreateRotationY(_camAngle);

           //_cubeTransform.Translation = new float3(0, 5 * M.Sin(3 *TimeSinceStart), 0);
 
            _sceneRenderer.Render(RC);

            
            // Swap buffers: Show the contents of the backbuffer (containing the currently rendered frame) on the front buffer.
            Present();
        
        }


        // Is called when the window was resized
        public override void Resize()
        {
            // Set the new rendering area to the entire new windows size
            RC.Viewport(0, 0, Width, Height);

            // Create a new projection matrix generating undistorted images on the new aspect ratio.
            var aspectRatio = Width / (float)Height;

            // 0.25*PI Rad -> 45° Opening angle along the vertical direction. Horizontal opening angle is calculated based on the aspect ratio
            // Front clipping happens at 1 (Objects nearer than 1 world unit get clipped)
            // Back clipping happens at 2000 (Anything further away from the camera than 2000 world units gets clipped, polygons will be cut)
            var projection = float4x4.CreatePerspectiveFieldOfView(M.PiOver4, aspectRatio, 1, 20000);
            RC.Projection = projection;
        }
    }
}