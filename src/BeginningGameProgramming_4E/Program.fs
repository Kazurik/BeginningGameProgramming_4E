// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.
open SharpDX.Windows
open SharpDX.Direct3D9
open SharpDX.Direct3D
open SharpDX

let dxb v = SharpDX.Bool(v)

let createD3DObjects () = 
    let form = new RenderForm("SharpDX Test")
    form.TopMost <- true
    form.FormBorderStyle <- System.Windows.Forms.FormBorderStyle.None

    let d3d = new Direct3D9.Direct3D()
    let d3dpp = new PresentParameters(form.Width, form.Height, Windowed = Bool true, SwapEffect = SwapEffect.Discard, BackBufferCount = 1, BackBufferFormat = Format.X8R8G8B8, DeviceWindowHandle = form.Handle)
    let d3ddev = new Device(d3d, 0, DeviceType.Hardware, form.Handle, CreateFlags.HardwareVertexProcessing, d3dpp)

    (form, d3d, d3ddev)

[<EntryPoint>]
let main argv = 
    let form, d3d, d3ddev = createD3DObjects()
    
    let cornflowerBlue = ColorBGRA.FromRgba(Color.CornflowerBlue.ToRgba())
    RenderLoop.Run(form, (fun () -> d3ddev.Clear(ClearFlags.Target ||| ClearFlags.ZBuffer, cornflowerBlue, float32 1, 0)
                                    d3ddev.BeginScene()
                                    d3ddev.EndScene()
                                    d3ddev.Present()))
    0 // return an integer exit code