  
-- MoonSharp Documentation - http://www.moonsharp.org/getting_started.html
-- ImGui - https://github.com/ocornut/imgui
-- ImGui.NET - https://github.com/mellinoe/ImGui.NET

-- Main Exeuction Point
function draw()
    draw_menu()
end

function draw_menu()
    local timestamp = util_getValue("timestamp", "")

    imgui.SetNextWindowSize(imgui.CreateVector2(399, 146))
    imgui.Begin("Go To Objects", imgui_window_flags.NoResize)

    imgui.TextWrapped("This dialog will allow you to go to and select objects that are copied to the clipboard.")
    
    imgui.Dummy(imgui.CreateVector2(0, 5))

    imgui.TextWrapped("To copy a timestamp to your clipboard, select a few objects, and press CTRL + C.")

    imgui.Dummy(imgui.CreateVector2(0, 8))
    
    imgui.Text("Timestamp:")

    imgui.SameLine()
    imgui.Dummy(imgui.CreateVector2(-10, -5))
    imgui.SameLine()

    _, timestamp = imgui.InputText("", timestamp, 2000, imgui_input_text_flags.EnterReturnsTrue)

    imgui.SameLine()
    imgui.Dummy(imgui.CreateVector2(-18, 0))
    imgui.SameLine()

    if imgui.Button("Go!") then
        actions.GoToObjects(timestamp)
        timestamp = ""
    end

    if utils.IsKeyPressed(keys.Enter) then
        actions.GoToObjects(timestamp)
        timestamp = ""
    end

    state.IsWindowHovered = imgui.IsWindowHovered() or imgui.IsItemFocused()
    imgui.End();

    state.SetValue("timestamp", timestamp)
end

function util_getValue(label, defaultValue)
    return state.GetValue(label) or defaultValue
end