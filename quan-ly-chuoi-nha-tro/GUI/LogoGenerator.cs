using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace quan_ly_chuoi_nha_tro.GUI
{
    /// <summary>
    /// Tạo logo cho ứng dụng - Giường ngủ (Bed with Moon & Stars)
    /// </summary>
    public static class LogoGenerator
    {
        /// <summary>
        /// Tạo Logo Quản Lý Nhà Trọ - Design Giường Ngủ
        /// </summary>
        public static Bitmap GenerateLogo(int size = 200)
        {
            Bitmap bitmap = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.White);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                float scale = size / 200f;

                // Mặt trăng (xanh chính)
                using (Brush moonBrush = new SolidBrush(Color.FromArgb(0, 122, 204))) // #007ACC
                {
                    // Hình cung tròn cho mặt trăng
                    g.FillEllipse(moonBrush, (int)(120 * scale), (int)(25 * scale), (int)(50 * scale), (int)(50 * scale));
                }

                // Đám mây (nhỏ)
                DrawCloud(g, 85, 55, 25, 15, scale);

                // Sao 1 (lớn)
                DrawStar(g, 165, 40, 12, scale);

                // Sao 2 (nhỏ)
                DrawStar(g, 175, 60, 8, scale);

                // Thanh ngoài giường (khung trên)
                using (Pen framePen = new Pen(Color.FromArgb(80, 80, 80), 8 * scale))
                {
                    // Thanh trái
                    g.DrawLine(framePen, (int)(50 * scale), (int)(85 * scale), (int)(50 * scale), (int)(140 * scale));
                    // Thanh phải
                    g.DrawLine(framePen, (int)(150 * scale), (int)(85 * scale), (int)(150 * scale), (int)(140 * scale));
                }

                // Đầu giường (2 cái gối)
                // Gối trái
                using (Brush pillowBrush = new SolidBrush(Color.FromArgb(220, 220, 220)))
                {
                    g.FillRectangle(pillowBrush, (int)(60 * scale), (int)(75 * scale), (int)(30 * scale), (int)(25 * scale));
                }
                using (Pen pillowPen = new Pen(Color.FromArgb(100, 100, 100), 2 * scale))
                {
                    g.DrawRectangle(pillowPen, (int)(60 * scale), (int)(75 * scale), (int)(30 * scale), (int)(25 * scale));
                }

                // Gối phải
                using (Brush pillowBrush = new SolidBrush(Color.FromArgb(220, 220, 220)))
                {
                    g.FillRectangle(pillowBrush, (int)(110 * scale), (int)(75 * scale), (int)(30 * scale), (int)(25 * scale));
                }
                using (Pen pillowPen = new Pen(Color.FromArgb(100, 100, 100), 2 * scale))
                {
                    g.DrawRectangle(pillowPen, (int)(110 * scale), (int)(75 * scale), (int)(30 * scale), (int)(25 * scale));
                }

                // Mặt giường (phần ngoài - xám đậm)
                using (Brush bedBrush = new SolidBrush(Color.FromArgb(100, 100, 100)))
                {
                    g.FillRectangle(bedBrush, (int)(45 * scale), (int)(100 * scale), (int)(110 * scale), (int)(50 * scale));
                }

                // Mặt giường (phần trong - xanh sáng)
                using (Brush mattressBrush = new SolidBrush(Color.FromArgb(135, 206, 235))) // #87CEEB - Xanh nhạt
                {
                    g.FillRectangle(mattressBrush, (int)(50 * scale), (int)(105 * scale), (int)(100 * scale), (int)(40 * scale));
                }

                // Đường viền giường
                using (Pen bedPen = new Pen(Color.FromArgb(80, 80, 80), 3 * scale))
                {
                    g.DrawRectangle(bedPen, (int)(50 * scale), (int)(105 * scale), (int)(100 * scale), (int)(40 * scale));
                }

                // Chân giường (dưới)
                using (Brush legBrush = new SolidBrush(Color.FromArgb(100, 100, 100)))
                {
                    // Chân trái
                    g.FillRectangle(legBrush, (int)(60 * scale), (int)(145 * scale), (int)(15 * scale), (int)(15 * scale));
                    // Chân phải
                    g.FillRectangle(legBrush, (int)(125 * scale), (int)(145 * scale), (int)(15 * scale), (int)(15 * scale));
                }

                // Mặt giường phần trước
                using (Brush baseBrush = new SolidBrush(Color.FromArgb(80, 80, 80)))
                {
                    g.FillRectangle(baseBrush, (int)(45 * scale), (int)(145 * scale), (int)(110 * scale), (int)(12 * scale));
                }
            }

            return bitmap;
        }

        /// <summary>
        /// Vẽ đám mây
        /// </summary>
        private static void DrawCloud(Graphics g, int x, int y, int width, int height, float scale)
        {
            using (Brush cloudBrush = new SolidBrush(Color.FromArgb(200, 200, 200)))
            {
                // 3 vòng tròn để tạo hình đám mây
                g.FillEllipse(cloudBrush, (int)(x * scale), (int)(y * scale), (int)(width * 0.6f * scale), (int)(height * scale));
                g.FillEllipse(cloudBrush, (int)((x + width * 0.3f) * scale), (int)((y - height * 0.2f) * scale), (int)(width * 0.7f * scale), (int)(height * 1.2f * scale));
                g.FillEllipse(cloudBrush, (int)((x + width * 0.6f) * scale), (int)(y * scale), (int)(width * 0.6f * scale), (int)(height * scale));
            }
        }

        /// <summary>
        /// Vẽ sao 4 cánh
        /// </summary>
        private static void DrawStar(Graphics g, int x, int y, int size, float scale)
        {
            using (Brush starBrush = new SolidBrush(Color.FromArgb(100, 100, 100)))
            {
                // Sao 4 cánh (hình thoi + dấu cộng)
                float scaledSize = size * scale;
                float centerX = x * scale;
                float centerY = y * scale;

                // Cánh trên
                PointF[] topPoint = new PointF[]
                {
                    new PointF(centerX, centerY - scaledSize),
                    new PointF(centerX + scaledSize * 0.3f, centerY - scaledSize * 0.3f),
                    new PointF(centerX, centerY),
                    new PointF(centerX - scaledSize * 0.3f, centerY - scaledSize * 0.3f)
                };
                g.FillPolygon(starBrush, topPoint);

                // Cánh phải
                PointF[] rightPoint = new PointF[]
                {
                    new PointF(centerX + scaledSize, centerY),
                    new PointF(centerX + scaledSize * 0.3f, centerY + scaledSize * 0.3f),
                    new PointF(centerX, centerY),
                    new PointF(centerX + scaledSize * 0.3f, centerY - scaledSize * 0.3f)
                };
                g.FillPolygon(starBrush, rightPoint);

                // Cánh dưới
                PointF[] bottomPoint = new PointF[]
                {
                    new PointF(centerX, centerY + scaledSize),
                    new PointF(centerX - scaledSize * 0.3f, centerY + scaledSize * 0.3f),
                    new PointF(centerX, centerY),
                    new PointF(centerX + scaledSize * 0.3f, centerY + scaledSize * 0.3f)
                };
                g.FillPolygon(starBrush, bottomPoint);

                // Cánh trái
                PointF[] leftPoint = new PointF[]
                {
                    new PointF(centerX - scaledSize, centerY),
                    new PointF(centerX - scaledSize * 0.3f, centerY - scaledSize * 0.3f),
                    new PointF(centerX, centerY),
                    new PointF(centerX - scaledSize * 0.3f, centerY + scaledSize * 0.3f)
                };
                g.FillPolygon(starBrush, leftPoint);
            }
        }

        /// <summary>
        /// Tạo Logo Icon đơn giản
        /// </summary>
        public static Bitmap GenerateSimpleIcon(int size = 128)
        {
            Bitmap bitmap = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.White);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                float scale = size / 200f;

                // Mặt trăng
                using (Brush moonBrush = new SolidBrush(Color.FromArgb(0, 122, 204)))
                {
                    g.FillEllipse(moonBrush, (int)(110 * scale), (int)(20 * scale), (int)(40 * scale), (int)(40 * scale));
                }

                // Giường - đơn giản
                // Khung
                using (Pen framePen = new Pen(Color.FromArgb(80, 80, 80), 6 * scale))
                {
                    g.DrawLine(framePen, (int)(50 * scale), (int)(85 * scale), (int)(50 * scale), (int)(135 * scale));
                    g.DrawLine(framePen, (int)(150 * scale), (int)(85 * scale), (int)(150 * scale), (int)(135 * scale));
                }

                // Mặt giường
                using (Brush mattressBrush = new SolidBrush(Color.FromArgb(135, 206, 235)))
                {
                    g.FillRectangle(mattressBrush, (int)(55 * scale), (int)(95 * scale), (int)(90 * scale), (int)(35 * scale));
                }

                using (Pen bedPen = new Pen(Color.FromArgb(80, 80, 80), 2 * scale))
                {
                    g.DrawRectangle(bedPen, (int)(55 * scale), (int)(95 * scale), (int)(90 * scale), (int)(35 * scale));
                }
            }

            return bitmap;
        }
    }
}

