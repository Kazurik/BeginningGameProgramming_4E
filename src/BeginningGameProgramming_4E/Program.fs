// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.
open SharpDX.Windows
open SharpDX.Direct3D9
open SharpDX.Direct3D
open SharpDX.DirectInput
open SharpDX
open System
open SharpDX.XInput

let dxb v = SharpDX.Bool(v)

let createD3DObjects () = 
    let form = new RenderForm("SharpDX Test")
    form.FormBorderStyle <- System.Windows.Forms.FormBorderStyle.None

    let d3d = new Direct3D9.Direct3D()
    let dm = d3d.GetAdapterDisplayMode(0)
    form.Width <- 640
    form.Height <- 480
    let d3dpp = new PresentParameters(form.Width, form.Height, Windowed = Bool true, SwapEffect = SwapEffect.Discard, BackBufferCount = 1, BackBufferFormat = dm.Format, DeviceWindowHandle = form.Handle)
    let d3ddev = new SharpDX.Direct3D9.Device(d3d, 0, DeviceType.Hardware, form.Handle, CreateFlags.HardwareVertexProcessing, d3dpp)

    (form, d3d, d3ddev)

[<AbstractClass; Sealed>]
type ColorsH private () =
    static let fromColor (color:Color) = ColorBGRA.FromRgba(color.ToBgra())
    static let _red = fromColor Color.Red
    static let _rand = new System.Random()
    static member FromColor (color:Color) = fromColor color
    static member Red = _red
    static member RandomColor = new ColorBGRA(_rand.NextFloat(float32 0 , float32 1), _rand.NextFloat(float32 0 , float32 1), _rand.NextFloat(float32 0 , float32 1), float32 1)

   
let joystickToString (controller:Controller) =
    let gamepad = controller.GetState().Gamepad
    if gamepad.LeftTrigger > byte 0 then "Left Trigger"
    else if gamepad.RightTrigger > byte 0  then "Right Trigger"
    else if gamepad.LeftThumbX < - int16 10000 || gamepad.LeftThumbX > int16 10000 then "Left Thumb Stick"
    else if gamepad.RightThumbX < - int16 10000 || gamepad.RightThumbX > int16 10000 then "Right Thumb Stick"
    else if gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadUp) then "DPAD Up"
    else if gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadDown) then "DPAD Down"
    else if gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadLeft) then "DPAD Left"
    else if gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadRight) then "DPAD Right"
    else if gamepad.Buttons.HasFlag(GamepadButtonFlags.Start) then "Start button"
    else if gamepad.Buttons.HasFlag(GamepadButtonFlags.LeftThumb) then "Left Thunmb"
    else if gamepad.Buttons.HasFlag(GamepadButtonFlags.RightShoulder) then "Right Thumb"
    else if gamepad.Buttons.HasFlag(GamepadButtonFlags.LeftShoulder) then "Left Shoulder"
    else if gamepad.Buttons.HasFlag(GamepadButtonFlags.RightShoulder) then "Right Shoulder"
    else if gamepad.Buttons.HasFlag(GamepadButtonFlags.A) then "A Button"
    else if gamepad.Buttons.HasFlag(GamepadButtonFlags.B) then "B Button"
    else if gamepad.Buttons.HasFlag(GamepadButtonFlags.X) then "X Button"
    else if gamepad.Buttons.HasFlag(GamepadButtonFlags.Y) then "Y Button"
    else ""

[<EntryPoint>]
let main argv = 
    let form, d3d, d3ddev = createD3DObjects()
        
    (*
    let dinput = new DirectInput()
    let dikeyboard = new Keyboard(dinput)
    dikeyboard.SetCooperativeLevel(form.Handle, CooperativeLevel.NonExclusive ||| CooperativeLevel.Background)
    form.Show() // The window must be fisible for Acquire to work
    dikeyboard.Acquire()
    let dimouse = new Mouse(dinput)
    dimouse.SetCooperativeLevel(form.Handle, CooperativeLevel.NonExclusive ||| CooperativeLevel.Foreground)
    let controller = new SharpDX.XInput.Controller(UserIndex.One)
    let vibration = new Vibration()
    if(controller.IsConnected) then
        controller.SetVibration(new Vibration(LeftMotorSpeed = uint16 65535, RightMotorSpeed = uint16 65535)) |> ignore
    *)

    let controllers = [UserIndex.One;UserIndex.Two;UserIndex.Three;UserIndex.Four;] |> List.map (fun i -> new Controller(i))
    let cornflowerBlue = ColorBGRA.FromRgba(Color.CornflowerBlue.ToRgba())
    RenderLoop.Run(form, (fun () -> d3ddev.Clear(ClearFlags.Target ||| ClearFlags.ZBuffer, cornflowerBlue, float32 1, 0)
                                    d3ddev.BeginScene()
                                    (*
                                    if form.Focused && form.Visible then 
                                        let kstate = dikeyboard.GetCurrentState()
                                        if (kstate.PressedKeys |> Seq.exists (fun k -> k = Key.Escape)) then form.Close()
                                    *)
                                    controllers |> List.filter (fun c -> c.IsConnected) |> List.map joystickToString |> 
                                      List.iter (fun s -> if s.Length > 0 then System.Windows.Forms.MessageBox.Show(s, "Controller") |> ignore)
                                    controllers |> List.filter (fun c -> c.IsConnected) |> 
                                      List.iter (fun c -> if c.GetState().Gamepad.Buttons.HasFlag(GamepadButtonFlags.Back) then form.Close())
                                    d3ddev.EndScene()
                                    d3ddev.Present()))

    (*
    dikeyboard.Unacquire()
    dimouse.Unacquire()
    *)
    d3d.Dispose()
    d3ddev.Dispose()
    0 // return an integer exit code