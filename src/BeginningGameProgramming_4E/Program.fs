// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.
open SharpDX.Windows
open SharpDX.Direct3D9
open SharpDX.Direct3D
open SharpDX
open System

let dxb v = SharpDX.Bool(v)

let createD3DObjects () = 
    let form = new RenderForm("SharpDX Test")
    //form.TopMost <- true
    form.FormBorderStyle <- System.Windows.Forms.FormBorderStyle.None

    let d3d = new Direct3D9.Direct3D()
    let dm = d3d.GetAdapterDisplayMode(0)
    form.Width <- dm.Width
    form.Height <- dm.Height
    let d3dpp = new PresentParameters(form.Width, form.Height, Windowed = Bool true, SwapEffect = SwapEffect.Discard, BackBufferCount = 1, BackBufferFormat = dm.Format, DeviceWindowHandle = form.Handle)
    let d3ddev = new Device(d3d, 0, DeviceType.Hardware, form.Handle, CreateFlags.HardwareVertexProcessing, d3dpp)

    (form, d3d, d3ddev)

[<AbstractClass; Sealed>]
type ColorsH private () =
    static let fromColor (color:Color) = ColorBGRA.FromRgba(color.ToBgra())
    static let _red = fromColor Color.Red
    static let _rand = new System.Random()
    static member FromColor (color:Color) = fromColor color
    static member Red = _red
    static member RandomColor = new ColorBGRA(_rand.NextFloat(float32 0 , float32 1), _rand.NextFloat(float32 0 , float32 1), _rand.NextFloat(float32 0 , float32 1), float32 1)

    
[<EntryPoint>]
let main argv = 
    let form, d3d, d3ddev = createD3DObjects()
    
    let backbuffer = d3ddev.GetBackBuffer(0, 0)
    let rect = new Nullable<Rectangle>(new Rectangle(100, 90, 100, 90))
    let surface = Surface.CreateOffscreenPlain(d3ddev, 100, 100, Format.X8R8G8B8, Pool.Default)

    let rand = new System.Random()
    let cornflowerBlue = ColorBGRA.FromRgba(Color.CornflowerBlue.ToRgba())
    RenderLoop.Run(form, (fun () -> d3ddev.Clear(ClearFlags.Target ||| ClearFlags.ZBuffer, cornflowerBlue, float32 1, 0)
                                    d3ddev.BeginScene()
                                    d3ddev.ColorFill(surface, ColorsH.RandomColor)
                                    d3ddev.StretchRectangle(surface, Nullable(), backbuffer, rect, TextureFilter.None)
                                    d3ddev.EndScene()
                                    d3ddev.Present()))
    0 // return an integer exit code