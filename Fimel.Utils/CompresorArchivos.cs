using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Xobject;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Fimel.Utils
{
    /// <summary>
    /// Reduce el peso de archivos subidos (imágenes y PDF) bajando calidad/resolución
    /// hasta acercarse a un tamaño objetivo (por defecto ~300 KB).
    /// Si el archivo ya pesa menos que el objetivo, o no se logra achicar, se devuelve tal cual.
    /// </summary>
    public static class CompresorArchivos
    {
        public const long TamanoObjetivoBytes = 300 * 1024;

        // Imágenes dentro de un PDF más chicas que esto no se tocan (logos, firmas, íconos).
        private const int TamanoMinimoImagenPdf = 20 * 1024;

        private static readonly string[] ExtensionesImagen = { ".jpg", ".jpeg", ".png", ".webp", ".bmp", ".gif", ".tif", ".tiff" };

        public record Resultado(byte[] Contenido, string MimeType, string NombreArchivo, bool Comprimido);

        public static Resultado Comprimir(byte[] contenido, string? mimeType, string? nombreArchivo, long objetivo = TamanoObjetivoBytes)
        {
            string mime = mimeType ?? "application/octet-stream";
            string nombre = nombreArchivo ?? "archivo";
            var original = new Resultado(contenido, mime, nombre, false);

            if (contenido.Length <= objetivo)
                return original;

            try
            {
                string extension = Path.GetExtension(nombre).ToLowerInvariant();

                if (mime.Equals("application/pdf", StringComparison.OrdinalIgnoreCase) || extension == ".pdf")
                {
                    byte[]? pdf = ComprimirPdf(contenido, objetivo);
                    if (pdf != null && pdf.Length < contenido.Length)
                        return new Resultado(pdf, "application/pdf", nombre, true);
                }
                else if (mime.StartsWith("image/", StringComparison.OrdinalIgnoreCase) || ExtensionesImagen.Contains(extension))
                {
                    byte[]? jpg = ComprimirImagen(contenido, objetivo);
                    if (jpg != null && jpg.Length < contenido.Length)
                        return new Resultado(jpg, "image/jpeg", Path.ChangeExtension(nombre, ".jpg"), true);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"CompresorArchivos: no se pudo comprimir '{nombre}' ({contenido.Length} bytes): {ex.Message}");
            }

            return original;
        }

        #region Imágenes

        private static readonly int[] DimensionesImagen = { 2000, 1600, 1280, 1024, 800 };
        private static readonly int[] CalidadesImagen = { 80, 70, 60, 50 };

        private static byte[]? ComprimirImagen(byte[] contenido, long objetivo)
        {
            using Image<Rgb24> imagen = CargarSinTransparencia(contenido);
            byte[]? masChico = null;

            foreach (int dimension in DimensionesImagen)
            {
                using Image<Rgb24> redimensionada = Redimensionar(imagen, dimension);
                foreach (int calidad in CalidadesImagen)
                {
                    byte[] jpg = CodificarJpeg(redimensionada, calidad);
                    if (masChico == null || jpg.Length < masChico.Length)
                        masChico = jpg;
                    if (jpg.Length <= objetivo)
                        return jpg;
                }
            }

            return masChico;
        }

        /// <summary>Carga la imagen, la orienta según EXIF (fotos de celular) y aplana la transparencia sobre blanco.</summary>
        private static Image<Rgb24> CargarSinTransparencia(byte[] contenido)
        {
            using Image<Rgba32> rgba = Image.Load<Rgba32>(contenido);
            rgba.Mutate(x => x.AutoOrient().BackgroundColor(Color.White));
            Image<Rgb24> rgb = rgba.CloneAs<Rgb24>();
            rgb.Metadata.ExifProfile = null;
            rgb.Metadata.IptcProfile = null;
            rgb.Metadata.XmpProfile = null;
            return rgb;
        }

        private static Image<Rgb24> Redimensionar(Image<Rgb24> imagen, int dimensionMaxima)
        {
            Image<Rgb24> copia = imagen.Clone();
            if (imagen.Width > dimensionMaxima || imagen.Height > dimensionMaxima)
            {
                copia.Mutate(x => x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(dimensionMaxima, dimensionMaxima)
                }));
            }
            return copia;
        }

        private static byte[] CodificarJpeg(Image<Rgb24> imagen, int calidad)
        {
            using var ms = new MemoryStream();
            imagen.SaveAsJpeg(ms, new JpegEncoder { Quality = calidad });
            return ms.ToArray();
        }

        #endregion

        #region PDF

        // Niveles de compresión para las imágenes internas del PDF (escaneos, fotos): dimensión máxima y calidad JPEG.
        private static readonly (int Dimension, int Calidad)[] NivelesPdf =
        {
            (1800, 75), (1500, 65), (1240, 55), (1000, 45), (800, 40)
        };

        private static byte[]? ComprimirPdf(byte[] contenido, long objetivo)
        {
            byte[]? masChico = null;

            foreach (var (dimension, calidad) in NivelesPdf)
            {
                byte[] pdf = ReescribirPdf(contenido, dimension, calidad);
                if (masChico == null || pdf.Length < masChico.Length)
                    masChico = pdf;
                if (pdf.Length <= objetivo)
                    break;
            }

            return masChico;
        }

        /// <summary>
        /// Reescribe el PDF con compresión completa y recodifica como JPEG las imágenes grandes
        /// (que es lo que normalmente hace pesar un examen escaneado).
        /// </summary>
        private static byte[] ReescribirPdf(byte[] contenido, int dimensionMaxima, int calidad)
        {
            using var salida = new MemoryStream();
            var reader = new PdfReader(new MemoryStream(contenido));
            reader.SetUnethicalReading(true);
            var writer = new PdfWriter(salida, new WriterProperties()
                .SetFullCompressionMode(true)
                .SetCompressionLevel(9));

            using (var pdf = new PdfDocument(reader, writer))
            {
                int total = pdf.GetNumberOfPdfObjects();

                // Las máscaras de transparencia (SMask/Mask) deben seguir en escala de grises: no se tocan.
                var mascaras = new HashSet<int>();
                for (int i = 1; i < total; i++)
                {
                    if (pdf.GetPdfObject(i) is PdfStream s && PdfName.Image.Equals(s.GetAsName(PdfName.Subtype)))
                    {
                        AgregarReferencia(mascaras, s.Get(PdfName.SMask, false));
                        AgregarReferencia(mascaras, s.Get(PdfName.Mask, false));
                    }
                }

                for (int i = 1; i < total; i++)
                {
                    if (mascaras.Contains(i))
                        continue;
                    if (pdf.GetPdfObject(i) is not PdfStream stream)
                        continue;
                    if (!PdfName.Image.Equals(stream.GetAsName(PdfName.Subtype)))
                        continue;

                    try
                    {
                        RecomprimirImagenPdf(stream, dimensionMaxima, calidad);
                    }
                    catch
                    {
                        // Formatos que no sabemos decodificar (JBIG2, JPX, CCITT, etc.): se dejan como están.
                    }
                }
            }

            return salida.ToArray();
        }

        private static void AgregarReferencia(HashSet<int> destino, PdfObject? objeto)
        {
            PdfIndirectReference? referencia = objeto?.GetIndirectReference() ?? (objeto as PdfIndirectReference);
            if (referencia != null)
                destino.Add(referencia.GetObjNumber());
        }

        private static void RecomprimirImagenPdf(PdfStream stream, int dimensionMaxima, int calidad)
        {
            if (stream.GetLength() < TamanoMinimoImagenPdf)
                return;

            PdfBoolean? esMascara = stream.GetAsBoolean(PdfName.ImageMask);
            if (esMascara != null && esMascara.GetValue())
                return;

            if (stream.ContainsKey(PdfName.SMask) || stream.ContainsKey(PdfName.Mask))
                return; // Tiene transparencia: recodificar en JPEG la perdería.

            PdfNumber? bits = stream.GetAsNumber(PdfName.BitsPerComponent);
            if (bits != null && bits.IntValue() == 1)
                return; // Imágenes de 1 bit (fax/B&N) ya son livianas y se verían mal en JPEG.

            var xobject = new PdfImageXObject(stream);
            byte[] bytesImagen = xobject.GetImageBytes();

            using Image<Rgb24> imagen = CargarSinTransparencia(bytesImagen);
            using Image<Rgb24> redimensionada = Redimensionar(imagen, dimensionMaxima);
            byte[] jpg = CodificarJpeg(redimensionada, calidad);

            if (jpg.Length >= stream.GetLength())
                return; // No vale la pena: la imagen original ya era más liviana.

            stream.Clear();
            stream.SetData(jpg, false);
            stream.SetCompressionLevel(CompressionConstants.NO_COMPRESSION);
            stream.Put(PdfName.Type, PdfName.XObject);
            stream.Put(PdfName.Subtype, PdfName.Image);
            stream.Put(PdfName.Filter, PdfName.DCTDecode);
            stream.Put(PdfName.Width, new PdfNumber(redimensionada.Width));
            stream.Put(PdfName.Height, new PdfNumber(redimensionada.Height));
            stream.Put(PdfName.BitsPerComponent, new PdfNumber(8));
            stream.Put(PdfName.ColorSpace, PdfName.DeviceRGB);
        }

        #endregion
    }
}
