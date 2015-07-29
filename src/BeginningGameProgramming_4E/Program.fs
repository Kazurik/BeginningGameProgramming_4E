// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.
open SharpDX.Windows
open SharpDX.Direct3D9
open SharpDX.Direct3D
open SharpDX.DirectInput
open SharpDX
open System
open SharpDX.XInput
open System.Windows.Forms

let dxb v = SharpDX.Bool(v)

let width = 1024
let APPTITLE = "Bomb Catcher Game"
let createD3DObjects () = 
    let form = new RenderForm(APPTITLE)
    //form.FormBorderStyle <- System.Windows.Forms.FormBorderStyle.None

    let d3d = new Direct3D9.Direct3D()
    let dm = d3d.GetAdapterDisplayMode(0)
    form.Width <- width
    form.Height <- 768
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


type Sprite2D = {X:float;Y:float;Width:int;Height:int;VelX:float;VelY:float}
let defaultSprite2D = {X=0.0;Y=0.0;Width=0;Height=0;VelX=0.0;VelY=0.0}

let rand = new System.Random()
type Point = {X:float; Y:float}
let resetBomb () = {X = float (rand.Next(0, width - 128)); Y = 0.0;}

let DrawSurface (dest:Surface) x y (source:Surface) (d3ddev:Direct3D9.Device) =
    //get width/height from source surface
    let desc = source.Description

    //create rects for drawing
    let source_rect = new Nullable<Rectangle> (new Rectangle(0, 0, desc.Width, desc.Height))
    let dest_rect = new Nullable<Rectangle> (new Rectangle(x, y, desc.Width, desc.Height))
    //let backbuffer = d3ddev.GetBackBuffer(0, 0)
    //let rect = new Nullable<Rectangle>(new Rectangle(100, 90, 100, 90))
    //let surface = Surface.CreateOffscreenPlain(d3ddev, 100, 100, Format.X8R8G8B8, Pool.Default)
    //d3ddev.ColorFill(surface, ColorsH.RandomColor)
    d3ddev.StretchRectangle(source, Nullable(), dest, dest_rect, TextureFilter.None)
    //d3ddev.StretchRectangle(surface, Nullable(), surface, Nullable(), TextureFilter.None)    

let createDirectInputObjects (handle:nativeint) =
    let dinput = new DirectInput()
    let dikeyboard = new Keyboard(dinput)
    dikeyboard.SetCooperativeLevel(handle, CooperativeLevel.NonExclusive ||| CooperativeLevel.Background)
    dikeyboard.Acquire()
    let dimouse = new Mouse(dinput)
    dimouse.SetCooperativeLevel(handle, CooperativeLevel.NonExclusive ||| CooperativeLevel.Background)
    dimouse.Acquire()
    (dikeyboard, dimouse)

let SurfaceFromFile relativeFile d3ddev =
    let info = ImageInformation.FromFile(relativeFile)
    let mutable surface = Surface.CreateOffscreenPlain(d3ddev, info.Width, info.Height, Format.X8R8G8B8, Pool.Default)
    Surface.FromFile(surface, relativeFile, Filter.Default, 0)
    surface

[<EntryPoint>]
let main argv = 
    let form, d3d, d3ddev = createD3DObjects()

    let backbuffer = d3ddev.GetBackBuffer(0, 0)

    form.Show() // The form must be visible for the acquire methods to work
    let dikeyboard, dimouse = createDirectInputObjects form.Handle
    d3ddev.ShowCursor <- false

    //Make sure to download the resources from http://jharbour.com/wordpress/portfolio/beginning-game-programming-4th-edition/
    let bomb_surf = SurfaceFromFile(@"Resources\bomb.bmp") d3ddev
    let bucket_surf = SurfaceFromFile(@"Resources\bucket.bmp") d3ddev

    let mutable bomb = resetBomb ()
    let mutable bucket = {X=500.0;Y=630.0}

    let mutable vibration = 0
    let mutable score = 0

    let controllers = [UserIndex.One;UserIndex.Two;UserIndex.Three;UserIndex.Four;] |> List.map (fun i -> new Controller(i))
    let cornflowerBlue = ColorBGRA.FromRgba(Color.CornflowerBlue.ToRgba())

    RenderLoop.Run(form, (fun () -> let kstate = dikeyboard.GetCurrentState()

                                    // move the bomb down the screen
                                    bomb <- {bomb with Y = bomb.Y + 0.666}
                                    // see if bomb hit the floor
                                    if(bomb.Y  + float bomb_surf.Description.Height > float form.Height) then
                                        MessageBox.Show("Oh no, the bomb exploded!!", "YOU STINK") |> ignore
                                        form.Close()

                                    // move the bucket with the mouse
                                    let mstate = dimouse.GetCurrentState()
                                    let mx = mstate.X
                                    if(mx < 0) then
                                        bucket <- {bucket with X = bucket.X - 2.0}
                                    else if mx > 0 then
                                        bucket <- {bucket with X = bucket.X + 2.0}

                                    // move the bucket with the keyboard
                                    if kstate.PressedKeys |> Seq.exists (fun k -> k = Key.Left) then
                                        bucket <- {bucket with X = bucket.X - 2.0}
                                    else if kstate.PressedKeys |> Seq.exists (fun k -> k = Key.Right) then
                                        bucket <- {bucket with X = bucket.X + 2.0}

                                    // move the bucket with the controller
                                    controllers |> List.iter (fun c ->
                                        if c.IsConnected then
                                            let gamepad = c.GetState().Gamepad
                                            //Left and right Analog Sticks
                                            if gamepad.LeftThumbX < int16 -7500 then bucket <- {bucket with X = bucket.X - 2.0}
                                            else if gamepad.LeftThumbX > int16 7500 then bucket <- {bucket with X = bucket.X + 2.0}
                                            //Left and Right Triggers
                                            if gamepad.LeftTrigger > byte 128 then bucket <- {bucket with X = bucket.X - 2.0}
                                            else if gamepad.RightTrigger > byte 128 then bucket <- {bucket with X = bucket.X + 2.0}
                                            //Left and right D-Pad
                                            if gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadLeft) then bucket <- {bucket with X = bucket.X - 2.0}
                                            else if gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadRight) then bucket <- {bucket with X = bucket.X + 2.0}
                                            //Left and right shoulders
                                            if gamepad.Buttons.HasFlag(GamepadButtonFlags.LeftShoulder) then bucket <- {bucket with X = bucket.X - 2.0}
                                            else if gamepad.Buttons.HasFlag(GamepadButtonFlags.RightShoulder) then bucket <- {bucket with X = bucket.X + 2.0} )

                                    //Update Vibration
                                    if vibration > 0 then vibration <- vibration + 1
                                    controllers |> List.filter (fun c -> c.IsConnected) |> List.iter (fun c -> 
                                        if vibration > 20 then 
                                            c.SetVibration(new Vibration(LeftMotorSpeed=0us, RightMotorSpeed=0us)) |> ignore)
                                    if vibration > 20 then vibration <- 0

                                    // keep bucket inside the screen
                                    if bucket.X < 0.0 then bucket <- {bucket with X = 0.0}
                                    if bucket.X > float (width-128) then bucket <- {bucket with X = float (width - 128)}

                                    // see if bucket caught the bomb
                                    let cx = bomb.X + 64.0
                                    let cy = bomb.Y + 64.0
                                    if (cx > bucket.X && cx < bucket.X + 128.0 && cy > bucket.Y && cy < bucket.Y + 128.0) then
                                        // update and display score
                                        score <- score + 1
                                        form.Text <- APPTITLE + " [SCORE " + string score + "]"
                                        // vibrate the controller 
                                        controllers |> List.filter (fun c -> c.IsConnected) |> List.iter (fun c ->
                                            c.SetVibration(new Vibration(LeftMotorSpeed=65000us,RightMotorSpeed=65000us)) |> ignore)
                                        vibration <- 1; 
                                        // restart bomb
                                        bomb <- resetBomb ()

                                    if form.Visible then 
                                        // clear the backbuffer
                                        d3ddev.Clear(ClearFlags.Target ||| ClearFlags.ZBuffer, cornflowerBlue, float32 1, 0)
                                        //start rendering
                                        d3ddev.BeginScene()
                                    
                                        //draw the bomb
                                        DrawSurface backbuffer (int bomb.X) (int bomb.Y) bomb_surf d3ddev
                                        //draw the bucket
                                        DrawSurface backbuffer (int bucket.X) (int bucket.Y) bucket_surf d3ddev

                                        //stop rendering
                                        d3ddev.EndScene()
                                        d3ddev.Present()
                                    
                                    //escape key exits
                                    if (kstate.PressedKeys |> Seq.exists (fun k -> k = Key.Escape)) then form.Close()
                                    //controller Back button also exits
                                    controllers |> List.filter (fun c -> c.IsConnected) |> 
                                      List.iter (fun c -> if c.GetState().Gamepad.Buttons.HasFlag(GamepadButtonFlags.Back) then form.Close())) )

    
    dikeyboard.Unacquire()
    dimouse.Unacquire()
    bomb_surf.Dispose()
    bucket_surf.Dispose()
    d3d.Dispose()
    d3ddev.Dispose()
    0 // return an integer exit code