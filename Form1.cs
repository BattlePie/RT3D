using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Simple3D;

namespace RTX3d_test
{
    public partial class Form1 : Form
    {
        Point3D cam;
        Graphics gr;
        List<Ray3D> m_rays;
        List<Surface> m_walls;
        int angle;
        int n_rays_x;
        int n_rays_y;
        int far;
        int FOV_x;
        int FOV_y;
        int screen_width;
        int screen_height;
        int bounces;
        Omnilight m_light;

        public Form1()
        {
            InitializeComponent();
            screen_width = ClientRectangle.Width;
            screen_height = ClientRectangle.Height;
            n_rays_x = screen_width;
            n_rays_y = screen_height;
            bounces = 3;
            far = 170; // Дальность farplane
            FOV_x = screen_width; // Ширина farplane
            FOV_y = screen_height;
            cam = new Point3D(10, 0, 100);
            BackColor = Color.Black;
            gr = CreateGraphics();
            m_walls = new List<Surface>();
            m_rays = new List<Ray3D>();

            int floor = -10;
            int ceiling = 200;

            m_light = new Omnilight(new Point3D(105, 0, ceiling / 2), 200, Color.White, 200);

            // Окружение
            {
                /*Дальняя стена*/
                m_walls.Add(new Surface(new Point3D(200, -200, floor), new Point3D(200, -200, ceiling), new Point3D(200, 200, floor), Color.FromArgb(255, 10, 10, 10), 1f));
                m_walls.Add(new Surface(new Point3D(200, -200, ceiling), new Point3D(200, 200, ceiling), new Point3D(200, 200, floor), Color.FromArgb(255, 10, 10, 10), 1f));

                /* Задняя стена*/
                m_walls.Add(new Surface(new Point3D(0, -200, floor), new Point3D(0, 200, floor), new Point3D(0, -200, ceiling), Color.FromArgb(255, 100, 100, 10), 0.95f));
                m_walls.Add(new Surface(new Point3D(0, -200, ceiling), new Point3D(0, 200, floor), new Point3D(0, 200, ceiling), Color.FromArgb(255, 100, 100, 10), 0.95f));

                /*Правая стена*/
                m_walls.Add(new Surface(new Point3D(200, 200, floor), new Point3D(200, 200, ceiling), new Point3D(0, 200, ceiling), Color.Blue, 0f));
                m_walls.Add(new Surface(new Point3D(200, 200, floor), new Point3D(0, 200, ceiling), new Point3D(0, 200, floor), Color.Blue, 0f));

                /* Левая стена*/
                m_walls.Add(new Surface(new Point3D(200, -200, floor), new Point3D(0, -200, ceiling), new Point3D(200, -200, ceiling), Color.Red, 0f));
                m_walls.Add(new Surface(new Point3D(200, -200, floor), new Point3D(0, -200, floor), new Point3D(0, -200, ceiling), Color.Red, 0f));

                /* Пол*/
                m_walls.Add(new Surface(new Point3D(200, 200, floor), new Point3D(0, 200, floor), new Point3D(200, -200, floor), Color.FromArgb(255, 150, 150, 150), 0.2f));
                m_walls.Add(new Surface(new Point3D(0, 200, floor), new Point3D(0, -200, floor), new Point3D(200, -200, floor), Color.FromArgb(255, 150, 150, 150), 0.2f));

                /* Потолок*/
                m_walls.Add(new Surface(new Point3D(0, 200, ceiling), new Point3D(200, 200, ceiling), new Point3D(200, -200, ceiling), Color.FromArgb(255, 50, 50, 50), 0.6f, 50));
                m_walls.Add(new Surface(new Point3D(0, 200, ceiling), new Point3D(200, -200, ceiling), new Point3D(0, -200, ceiling), Color.FromArgb(255, 50, 50, 50), 0.6f, 50));
            }

            MakeCube(new Point3D(10, 30, 40), new Point3D(140, 20, floor), 0f, Color.DarkRed);
            MakeCube(new Point3D(10, 30, 40), new Point3D(70, -50, floor), 0f, Color.DarkRed);
            MakeCube(new Point3D(10, 30, 40), new Point3D(140, -50, floor), 0f, Color.DarkGreen);
            MakeCube(new Point3D(10, 30, 40), new Point3D(70, 20, floor), 0f, Color.DarkGreen);
            //MakeCube(new Point3D(20, 40, 10), new Point3D(100, -100, 30), 0f, Color.DarkRed);
            

            SetRays(m_rays);
            FillT(m_rays);
        }
        public class Ray3D : Vector
        {
            public float t = 1000;
            public Surface hit_wall;
            public Point3D hit_point;
            public Lightray light;
            public Ray3D reflected_ray;
            public float bounce;
            public Ray3D(Point3D input_starting_point, Point3D input_end) 
                : base(input_starting_point, input_end)
            {
               
            }

            public Color CalculateColor()
            {
                float reflection_distance = 0;
                Color col = Color.Black;

                if (reflected_ray != null && reflected_ray.hit_wall != null)
                {
                    col = reflected_ray.CalculateColor();
                    reflection_distance = Math.Max(0, hit_wall.reflection_distance - Length(new Vector(reflected_ray.start, reflected_ray.hit_point))) / hit_wall.reflection_distance;
                }
                Color c = Color.Black;

                if(light != null)
                {
                    float light_length = Length(light);
                    float fallof_power = (light.light.power - light_length) / light.light.power;
                    float light_power = Math.Max(0, fallof_power);//fallof_power > 0 ? 1 / fallof_power : 1 ;

                    c = Color.FromArgb(Math.Min(255, (int)(light_power * light.light.color.A) + hit_wall.color.A),
                        Math.Min(255, (int)(light_power * light.light.color.R) + hit_wall.color.R),
                        Math.Min(255, (int)(light_power * light.light.color.G) + hit_wall.color.G),
                        Math.Min(255, (int)(light_power * light.light.color.B) + hit_wall.color.B));
                }
                return Color.FromArgb(Math.Min(255, (int)(c.A + col.A * hit_wall.reflectivity * reflection_distance)),
                                      Math.Min(255, (int)(c.R + col.R * hit_wall.reflectivity * reflection_distance)),
                                      Math.Min(255, (int)(c.G + col.G * hit_wall.reflectivity * reflection_distance)),
                                      Math.Min(255, (int)(c.B + col.B * hit_wall.reflectivity * reflection_distance)));
            }
            public void CalcReflection()
            {
                Point3D zero = new Point3D(0, 0, 0);
                Vector n = new Vector(zero, new Point3D(hit_wall.A, hit_wall.B, hit_wall.C));
                n.Normalize();
                float sm = ScolarMult(this, n);
                Vector denum = new Vector(zero, new Point3D(n.relative_end.x * 2 * sm,
                                                            n.relative_end.y * 2 * sm, 
                                                            n.relative_end.z * 2 * sm));

                reflected_ray = new Ray3D(hit_point,
                    new Point3D(relative_end.x - denum.relative_end.x + hit_point.x,
                    relative_end.y - denum.relative_end.y + hit_point.y,
                    relative_end.z - denum.relative_end.z + hit_point.z));
            }
        }
        public class Surface : Polygon
        {
            public float reflectivity;
            public float reflection_distance;
            public Color color;

            public Surface(Point3D vertex1, Point3D vertex2, Point3D vertex3, Color color, float reflec = 0f,float ref_dist = 300f)
                :base( vertex1,  vertex2,  vertex3)
            {
                reflectivity = reflec;
                reflection_distance = ref_dist;
                this.color = color;
            }
        }
        public class Lightray : Vector
        {
            public Omnilight light;
            public Lightray(Point3D input_starting_point, Point3D input_end)
                : base(input_starting_point, input_end)
            {

            }
        }

        public struct Omnilight
        {
            public Point3D pos;
            public float power;
            public float fallof_distance;
            public Color color;
            public Omnilight(Point3D pos, int power, Color color, float fallof_distance)
            {
                this.pos = pos;
                this.power = power;
                this.color = color;
                this.fallof_distance = fallof_distance;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        
        }
        public void SetRays(List<Ray3D> vectors)
        {
            for (int j = 0; j < n_rays_y; j++)
            {
                for (int i = 0; i < n_rays_x; i++)
                {
                    Ray3D r = new Ray3D(cam,
                        new Point3D(far,
                                    -FOV_x / 2 + (FOV_x / n_rays_x) * i,
                                    -FOV_y / 2 + (FOV_y / n_rays_y) * j));
                    
                    vectors.Add(r);
                }
            }

        }
        public void FillT(List<Ray3D> rays)
        {
            foreach (Ray3D r in rays)
            {
                TestRay(r);
                if(r.hit_wall != null)
                {
                    Ray3D rr = r;
                    for (int i = 0; i < bounces && rr.hit_wall != null; i++)
                    {
                        rr.CalcReflection();
                        TestRay(rr.reflected_ray, rr.hit_wall);
                        rr = rr.reflected_ray;
                    }
                }
                
            }
        }
        private void TestRay(Ray3D r, Surface ignore_wall = null)
        {
            foreach (Surface wall in m_walls)
            {
                if(wall == ignore_wall)
                    continue; 

                float pre_t = r.FindT(wall);
                if (pre_t < r.t && pre_t > 0)
                {
                    Point3D pnt = new Point3D(r.relative_end.x * pre_t + r.start.x, r.relative_end.y * pre_t + r.start.y, r.relative_end.z * pre_t + r.start.z);

                    if (r.OnWall(pnt, wall))
                    {
                        r.hit_wall = wall;
                        r.t = pre_t;
                        r.hit_point = pnt;
                    }
                }
            }

            if (r.hit_wall != null)
            {
                r.light = new Lightray(r.hit_point, m_light.pos);
                if (Vector.ScolarMult(new Point3D(r.hit_wall.A, r.hit_wall.B, r.hit_wall.C), r.light.relative_end) <= 0)
                {
                    r.light = null; 
                    return;
                }

                foreach (Polygon wall in m_walls)
                {
                    if (wall == r.hit_wall)
                        continue;

                    float pre_t = r.light.FindT(wall);

                    if (pre_t > 0 && pre_t <= 1)
                    {
                        Point3D pnt = new Point3D(r.light.relative_end.x * pre_t + r.light.start.x, r.light.relative_end.y * pre_t + r.light.start.y, r.light.relative_end.z * pre_t + r.light.start.z);

                        if (r.light.OnWall(pnt, wall))
                        {
                            r.light = null;
                            break;
                        }
                    }
                }
                if (r.light != null)
                {
                    r.light.light = m_light;
                }
            }

        }
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            for (int i = 0; i < n_rays_x; i++)
            {
                for (int j = 0; j < n_rays_y; j++)
                {
                    if (m_rays[j * n_rays_x + i].hit_wall != null)
                    {
                        Brush brush = new SolidBrush(m_rays[j * n_rays_x + i].CalculateColor());
                        gr.FillRectangle(brush, i, screen_height - j , 1, 1);
                    }
                }
            }
        }
        public class Pixel
        {
            public int width;
            public int height;
            public int x;
            public int y;
            public int brightness;
            public Pixel(int input_width, int input_height, int input_x, int input_y, int input_brightness)
            {
                width = input_width;
                height = input_height;
                x = input_x;
                y = input_y;
                brightness = input_brightness;
            }
        }
        public void MakeCube(Point3D size, Point3D offset, float reflectivity, Color color)
        {
            
           /* 000 011 010 */ m_walls.Add(new Surface(new Point3D(offset.x, offset.y, offset.z), new Point3D(offset.x, offset.y + size.y, offset.z + size.z), new Point3D(offset.x, offset.y + size.y, offset.z),color,reflectivity));
           /* 000 001 011 */ m_walls.Add(new Surface(new Point3D(offset.x, offset.y, offset.z), new Point3D(offset.x, offset.y, offset.z + size.z), new Point3D(offset.x, offset.y + size.y, offset.z + size.z), color, reflectivity));
           /* 001 101 111 */ m_walls.Add(new Surface(new Point3D(offset.x, offset.y, offset.z + size.z), new Point3D(offset.x + size.x, offset.y, offset.z + size.z), new Point3D(offset.x + size.x, offset.y + size.y, offset.z + size.z), color, reflectivity));
           /* 001 111 011 */ m_walls.Add(new Surface(new Point3D(offset.x, offset.y, offset.z + size.z), new Point3D(offset.x + size.x, offset.y + size.y, offset.z + size.z), new Point3D(offset.x, offset.y + size.y, offset.z + size.z), color, reflectivity));
           /* 101 110 111 */ m_walls.Add(new Surface(new Point3D(offset.x + size.x, offset.y , offset.z + size.z), new Point3D(offset.x + size.x, offset.y + size.y, offset.z), new Point3D(offset.x + size.x, offset.y + size.y, offset.z + size.z), color, reflectivity));
           /* 101 100 110 */ m_walls.Add(new Surface(new Point3D(offset.x + size.x, offset.y, offset.z + size.z), new Point3D(offset.x + size.x, offset.y, offset.z), new Point3D(offset.x + size.x, offset.y + size.y, offset.z), color, reflectivity));
           /* 100 010 110 */ m_walls.Add(new Surface(new Point3D(offset.x + size.x, offset.y, offset.z), new Point3D(offset.x, offset.y + size.y, offset.z), new Point3D(offset.x + size.x, offset.y + size.y, offset.z), color, reflectivity));
           /* 100 000 010 */ m_walls.Add(new Surface(new Point3D(offset.x + size.x, offset.y, offset.z), new Point3D(offset.x, offset.y, offset.z), new Point3D(offset.x, offset.y + size.y, offset.z), color, reflectivity));
           /* 000 101 001 */ m_walls.Add(new Surface(new Point3D(offset.x, offset.y, offset.z), new Point3D(offset.x + size.x, offset.y, offset.z + size.z), new Point3D(offset.x, offset.y, offset.z + size.z), color, reflectivity));
           /* 000 100 101 */ m_walls.Add(new Surface(new Point3D(offset.x, offset.y, offset.z), new Point3D(offset.x + size.x, offset.y, offset.z), new Point3D(offset.x + size.x, offset.y, offset.z + size.z), color, reflectivity));
           /* 011 111 010 */ m_walls.Add(new Surface(new Point3D(offset.x, offset.y + size.y, offset.z + size.z), new Point3D(offset.x + size.x, offset.y + size.y, offset.z + size.z), new Point3D(offset.x, offset.y + size.y, offset.z), color, reflectivity));
           /* 010 111 110 */ m_walls.Add(new Surface(new Point3D(offset.x, offset.y + size.y, offset.z), new Point3D(offset.x + size.x, offset.y + size.y, offset.z + size.z), new Point3D(offset.x + size.x, offset.y + size.y, offset.z), color, reflectivity));
        }
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            Ray3D r = m_rays[(screen_height - e.Y) * n_rays_x + e.X];
            r.hit_wall = null;
            r.t = 10000;
            TestRay(r);
            if (r.hit_wall != null)
            {
                Ray3D rr = r;
                for (int i = 0; i < bounces && rr.hit_wall != null; i++)
                {
                    rr.CalcReflection();
                    TestRay(rr.reflected_ray, rr.hit_wall);
                    rr = rr.reflected_ray;
                }
            }
            r.CalculateColor();
        }
    }
}
