-- MoonSharp Documentation - http://www.moonsharp.org/getting_started.html
-- ImGui - https://github.com/ocornut/imgui
-- ImGui.NET - https://github.com/mellinoe/ImGui.NET

-- Main Exeuction Point
function draw ()
    draw_fps_menu()
    draw_volume_menu()
    draw_color_menu()
end

-- Draws a menu to change and cache the state of a color value
function draw_color_menu()
    local color = state.GetValue("color");

    if color == nil then
        color = imgui.CreateVector3(0, 0, 0)
    end

    imgui.Begin("Color", 0)
    x, value = imgui.ColorEdit3("Test colour", color)
    imgui.End();

    state.SetValue("color", value)
end

-- Draws a menu to change and cache the state of a volume value
function draw_volume_menu()
    local volume = state.GetValue("volume");

    if volume == nil then
        volume = 50
    end

    imgui.Begin("Volume", 0)
    x, value = imgui.SliderInt("Change Volume", volume, 0, 100)
    imgui.End()

    state.SetValue("volume", value)
end

-- Draws a menu to display the current FPS
function draw_fps_menu()
    imgui.Begin("Elapsed Time", 0)
    imgui.TextWrapped("Delta Time: " .. state.DeltaTime)
    imgui.TextWrapped("FPS: " .. (1000 / state.DeltaTime))
    imgui.End();
end
