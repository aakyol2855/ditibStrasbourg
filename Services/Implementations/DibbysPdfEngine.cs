using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DitibStasbourg.Data;
using DitibStasbourg.Models;
using DitibStasbourg.Models.Enums;
using DitibStasbourg.Services.Interfaces;
using PdfSharp.Pdf;
using PdfSharp.Drawing;

namespace DitibStasbourg.Services.Implementations
{
    public class DibbysPdfEngine : IDibbysPdfEngine
    {
        private readonly ApplicationDbContext _context;

        public DibbysPdfEngine(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> GenerateLeavePdfAsync(int izinId)
        {
            var izin = await _context.GorevliIzinler
                .Include(i => i.Gorevli)
                    .ThenInclude(g => g.Unvan)
                .FirstOrDefaultAsync(i => i.Id == izinId);

            if (izin == null || izin.Gorevli == null)
            {
                throw new Exception("İzin veya görevli kaydı bulunamadı.");
            }

            // Set up document
            var document = new PdfDocument();
            var page = document.AddPage();
            page.Size = PdfSharp.PageSize.A4;
            var gfx = XGraphics.FromPdfPage(page);

            // Draw branding border
            var penBorder = new XPen(XColors.Navy, 1.5);
            gfx.DrawRectangle(penBorder, 40, 40, page.Width.Point - 80, page.Height.Point - 80);

            // Title block
            var fontTitle = new XFont("Arial", 16, XFontStyleEx.Bold);
            gfx.DrawString("DİTİB STRASBOURG DİN HİZMETLERİ ATAŞELİĞİ", fontTitle, XBrushes.Navy, new XRect(40, 60, page.Width.Point - 80, 30), XStringFormats.Center);
            
            var fontSubTitle = new XFont("Arial", 12, XFontStyleEx.Bold);
            gfx.DrawString("İZİN TALEP VE ONAY FORMU", fontSubTitle, XBrushes.Black, new XRect(40, 85, page.Width.Point - 80, 25), XStringFormats.Center);

            // Horizontal line
            gfx.DrawLine(new XPen(XColors.Navy, 1), 60, 115, page.Width.Point - 60, 115);

            // Content setup
            var fontLabel = new XFont("Arial", 11, XFontStyleEx.Bold);
            var fontValue = new XFont("Arial", 11, XFontStyleEx.Regular);

            double y = 140;
            double labelX = 80;
            double valueX = 260;
            double lineHeight = 30;

            void DrawRow(string label, string value)
            {
                gfx.DrawString(label, fontLabel, XBrushes.Black, labelX, y);
                gfx.DrawString(value ?? "—", fontValue, XBrushes.Black, valueX, y);
                
                // Draw a very subtle bottom line for separation
                gfx.DrawLine(new XPen(XColors.LightGray, 0.5), 60, y + 8, page.Width.Point - 60, y + 8);
                y += lineHeight;
            }

            string LocalizeIzinTuru(IzinTuru tur) => tur switch
            {
                IzinTuru.YillikIzin => "Yıllık İzin",
                IzinTuru.HastalikIzni => "Hastalık İzni",
                IzinTuru.DogumIzni => "Doğum İzni",
                IzinTuru.BabalikIzni => "Babalık İzni",
                IzinTuru.MazeretIzni => "Mazeret İzni",
                _ => tur.ToString()
            };

            string LocalizeOnayDurumu(OnayDurumu durum) => durum switch
            {
                OnayDurumu.Beklemede => "Beklemede",
                OnayDurumu.Onaylandi => "Onaylandı",
                OnayDurumu.Reddedildi => "Reddedildi",
                OnayDurumu.IptalEdildi => "İptal Edildi",
                _ => durum.ToString()
            };

            DrawRow("Görevli Adı Soyadı:", $"{izin.Gorevli.Ad} {izin.Gorevli.Soyad}");
            DrawRow("Görevi / Ünvanı:", izin.Gorevli.Unvan?.Ad ?? "Din Görevlisi");
            DrawRow("İzin Türü:", LocalizeIzinTuru(izin.IzinTuru));
            DrawRow("Başlangıç Tarihi:", izin.BaslangicTarihi.ToString("dd.MM.yyyy"));
            DrawRow("Bitiş Tarihi:", izin.BitisTarihi.ToString("dd.MM.yyyy"));
            DrawRow("Toplam Gün (Jours Ouvrables):", $"{izin.ToplamGun} Gün");
            DrawRow("İzin Adresi:", izin.IzinAdresi);
            DrawRow("İzin Telefonu:", izin.IzinTelefonu);
            DrawRow("Evrak / Belge No:", izin.EvrakNo);
            DrawRow("Talep Tarihi:", izin.TalepTarihi.ToString("dd.MM.yyyy"));
            DrawRow("Onay Durumu:", LocalizeOnayDurumu(izin.OnayDurumu));

            if (izin.OnayTarihi.HasValue)
            {
                DrawRow("Onay Tarihi:", izin.OnayTarihi.Value.ToString("dd.MM.yyyy"));
            }

            // Signatures block
            y = page.Height.Point - 200;
            gfx.DrawLine(new XPen(XColors.Navy, 1), 60, y - 20, page.Width.Point - 60, y - 20);

            var fontSignatureHeader = new XFont("Arial", 11, XFontStyleEx.Bold);
            gfx.DrawString("Görevli İmzası", fontSignatureHeader, XBrushes.Black, 100, y);
            gfx.DrawString("Yetkili Onayı / İmza", fontSignatureHeader, XBrushes.Black, page.Width.Point - 250, y);

            y += 50;
            var fontSignatureLine = new XFont("Arial", 9, XFontStyleEx.Italic);
            gfx.DrawString("Tarih: ____/____/________", fontSignatureLine, XBrushes.Black, 100, y);
            gfx.DrawString("Tarih: ____/____/________", fontSignatureLine, XBrushes.Black, page.Width.Point - 250, y);

            y += 25;
            gfx.DrawString("İmza: _____________________", fontSignatureLine, XBrushes.DarkGray, 100, y);
            gfx.DrawString("İmza: _____________________", fontSignatureLine, XBrushes.DarkGray, page.Width.Point - 250, y);

            // Footer note
            y = page.Height.Point - 65;
            var fontFooter = new XFont("Arial", 8, XFontStyleEx.Regular);
            gfx.DrawString("DİTİB Strasbourg - Leave Management System - Generated on " + DateTime.Now.ToString("dd.MM.yyyy HH:mm"), 
                fontFooter, XBrushes.Gray, new XRect(40, y, page.Width.Point - 80, 20), XStringFormats.Center);

            using (var ms = new MemoryStream())
            {
                document.Save(ms);
                return ms.ToArray();
            }
        }
    }
}
