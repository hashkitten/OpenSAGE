﻿namespace OpenSage.Gui.Elements
{
    public sealed class RadioButton : UIElement
    {
        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawText(Text);
        }
    }
}