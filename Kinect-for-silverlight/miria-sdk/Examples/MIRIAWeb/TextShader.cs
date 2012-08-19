using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System;

namespace MIRIAWeb
{
    public class TextShader : ShaderEffect
    {

        public TextShader()
        {

            Uri u = new Uri(@"/MIRIAWeb;component/textfade.ps", UriKind.Relative);

            PixelShader psCustom = new PixelShader();

            psCustom.UriSource = u;

            PixelShader = psCustom;

        }

    }
}