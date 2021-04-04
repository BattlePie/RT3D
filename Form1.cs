using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
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
        static Color ambient_color;

        public Form1()
        {
            InitializeComponent();
            screen_width = ClientRectangle.Width;
            screen_height = ClientRectangle.Height;
            n_rays_x = screen_width;
            n_rays_y = screen_height;
            bounces = 3;
            far = 170;
            ambient_color = Color.FromArgb(255, 15, 15, 15);
            FOV_x = screen_width; 
            FOV_y = screen_height;
            cam = new Point3D(10, 0, 100);
            gr = CreateGraphics();
            m_walls = new List<Surface>();
            m_rays = new List<Ray3D>();

            int floor = -10;
            int ceiling = 200;

            m_light = new Omnilight(new Point3D(105, 0, ceiling / 2), 400, Color.White, 200);

            // Окружение
            {
                /*Дальняя стена*/
                m_walls.Add(new Surface(new Shape(new Point3D(200, -200, floor), new Point3D(200, -200, ceiling), new Point3D(200, 200, floor)), new SurfaceParam(Color.Red, 1f, 300, 0.85f)));
                m_walls.Add(new Surface(new Shape(new Point3D(200, -200, ceiling), new Point3D(200, 200, ceiling), new Point3D(200, 200, floor)), new SurfaceParam(Color.Red, 1f, 300, 0.85f)));

                /* Задняя стена*/
                m_walls.Add(new Surface(new Shape(new Point3D(0, -200, floor), new Point3D(0, 200, floor), new Point3D(0, -200, ceiling)), new SurfaceParam(Color.FromArgb(255, 100, 100, 100), 0.95f)));
                m_walls.Add(new Surface(new Shape(new Point3D(0, -200, ceiling), new Point3D(0, 200, floor), new Point3D(0, 200, ceiling)), new SurfaceParam(Color.FromArgb(255, 100, 100, 100), 0.95f)));

                /*Правая стена*/
                m_walls.Add(new Surface(new Shape(new Point3D(200, 200, floor), new Point3D(200, 200, ceiling), new Point3D(0, 200, ceiling)), new SurfaceParam(Color.Blue, 0f)));
                m_walls.Add(new Surface(new Shape(new Point3D(200, 200, floor), new Point3D(0, 200, ceiling), new Point3D(0, 200, floor)), new SurfaceParam(Color.Blue, 0f)));

                /* Левая стена*/
                m_walls.Add(new Surface(new Shape(new Point3D(200, -200, floor), new Point3D(0, -200, ceiling), new Point3D(200, -200, ceiling)), new SurfaceParam(Color.Red, 0f)));
                m_walls.Add(new Surface(new Shape(new Point3D(200, -200, floor), new Point3D(0, -200, floor), new Point3D(0, -200, ceiling)), new SurfaceParam(Color.Red, 0f)));

                /* Пол*/
                m_walls.Add(new Surface(new Shape(new Point3D(200, 200, floor), new Point3D(0, 200, floor), new Point3D(200, -200, floor)), new SurfaceParam(Color.White, 0.1f)));
                m_walls.Add(new Surface(new Shape(new Point3D(0, 200, floor), new Point3D(0, -200, floor), new Point3D(200, -200, floor)), new SurfaceParam(Color.White, 0.1f)));

                /* Потолок*/
                m_walls.Add(new Surface(new Shape(new Point3D(0, 200, ceiling), new Point3D(200, 200, ceiling), new Point3D(200, -200, ceiling)), new SurfaceParam(Color.FromArgb(255, 50, 50, 50), 0.6f, 200)));
                m_walls.Add(new Surface(new Shape(new Point3D(0, 200, ceiling), new Point3D(200, -200, ceiling), new Point3D(0, -200, ceiling)), new SurfaceParam(Color.FromArgb(255, 50, 50, 50), 0.6f, 200)));
            }

            MakeCube(new Point3D(10, 30, 40), new Point3D(140, 20, floor), 0f, Color.Yellow);
            MakeCube(new Point3D(10, 30, 40), new Point3D(70, -50, floor), 0f, Color.DarkRed);
            MakeCube(new Point3D(10, 30, 40), new Point3D(140, -50, floor), 0f, Color.DarkGreen);
            MakeCube(new Point3D(10, 30, 40), new Point3D(70, 20, floor), 0f, Color.DarkBlue);
            //MakeCube(new Point3D(20, 40, 10), new Point3D(100, -100, 30), 0f, Color.DarkRed);

            SetRays();
            FillT();
            /*
            Thread thread1 = new Thread(new ThreadStart(SetRays()));
            Thread thread2 = new Thread(new ThreadStart(FillT()));
            thread1.Start();
            thread2.Start();*/
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
                Color reflection_color = Color.Black;

                if (reflected_ray != null && reflected_ray.hit_wall != null)
                {
                    reflection_color = reflected_ray.CalculateColor();
                    reflection_distance = Math.Max(0, hit_wall.parameters.reflection_distance - Length(new Vector(reflected_ray.start, reflected_ray.hit_point))) / hit_wall.parameters.reflection_distance;
                }
                Color c = ambient_color;

                if (light != null)
                {
                    float light_length = Length(light);
                    float fallof_power = (light.light.power - light_length) / light.light.power;
                    float light_power = Math.Max(0, fallof_power * fallof_power);

                    float p1 = light_power / 255.0f;
                    c = Color.FromArgb(255,
                                       Math.Min(255,(int)(hit_wall.parameters.color.R * (light.light.color.R * p1 + ambient_color.R / 255f))),
                                       Math.Min(255,(int)(hit_wall.parameters.color.G * (light.light.color.G * p1 + ambient_color.G / 255f))),
                                       Math.Min(255,(int)(hit_wall.parameters.color.B * (light.light.color.B * p1 + ambient_color.B / 255f))));
                }
                float reflection_coefficient = hit_wall.parameters.reflectivity * reflection_distance;

                float metallic_r = 1 - hit_wall.parameters.metalness * (1 - hit_wall.parameters.color.R / 255f);
                float metallic_g = 1 - hit_wall.parameters.metalness * (1 - hit_wall.parameters.color.G / 255f);
                float metallic_b = 1 - hit_wall.parameters.metalness * (1 - hit_wall.parameters.color.B / 255f);

                return Color.FromArgb(255,
                                      Math.Min(255, (int)(c.R + reflection_color.R * reflection_coefficient * metallic_r)),
                                      Math.Min(255, (int)(c.G + reflection_color.G * reflection_coefficient * metallic_g)),
                                      Math.Min(255, (int)(c.B + reflection_color.B * reflection_coefficient * metallic_b)));
            
            }
            public void CalcReflection()
            {
                Point3D zero = new Point3D(0, 0, 0);
                //Vector n = new Vector(zero, new Point3D(hit_wall.polygon.A, hit_wall.polygon.B, hit_wall.polygon.C));
                //n.Normalize();
                float sm = ScolarMult(this, hit_wall.shape.polygon.n);
                Vector denum = new Vector(zero, new Point3D(hit_wall.shape.polygon.n.relative_end.x * 2 * sm,
                                                            hit_wall.shape.polygon.n.relative_end.y * 2 * sm,
                                                            hit_wall.shape.polygon.n.relative_end.z * 2 * sm));

                reflected_ray = new Ray3D(hit_point,
                    new Point3D(relative_end.x - denum.relative_end.x + hit_point.x,
                    relative_end.y - denum.relative_end.y + hit_point.y,
                    relative_end.z - denum.relative_end.z + hit_point.z));
            }
        }
        public class Surface
        {
            public SurfaceParam parameters;
            public Shape shape;
            public Surface(Shape shape, SurfaceParam parameters)
            {
                this.parameters = parameters;
                this.shape = shape;
            }
        }
        public class SurfaceParam
        {
            public float reflectivity;
            public float reflection_distance;
            public float metalness;
            public Color color;
            public SurfaceParam(Color color, float reflectivity = 0f, float reflection_distance = 300f, float metalness = 0.1f)
            {
                this.reflectivity = reflectivity;
                this.reflection_distance = reflection_distance;
                this.color = color;
                this.metalness = metalness;
            }

        }
        public class Shape
        {
            public Polygon polygon;
            public Sphere sphere;
            public Shape(Point3D vertex1, Point3D vertex2, Point3D vertex3)
            {
                polygon = new Polygon(vertex1, vertex2, vertex3);
            }
            public Shape(Point3D center, float radius)
            {
                sphere = new Sphere(center,radius);
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
        public void SetRays()
        {
            for (int j = 0; j < n_rays_y; j++)
            {
                for (int i = 0; i < n_rays_x; i++)
                {
                    Ray3D r = new Ray3D(cam,
                        new Point3D(far,
                                    -FOV_x / 2 + (FOV_x / n_rays_x) * i,
                                    -FOV_y / 2 + (FOV_y / n_rays_y) * j));
                    
                    m_rays.Add(r);
                }
            }

        }
        public void FillT()
        {
            foreach (Ray3D r in m_rays)
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

                float pre_t = r.FindT(wall.shape.polygon);
                if (pre_t < r.t && pre_t > 0)
                {
                    Point3D pnt = new Point3D(r.relative_end.x * pre_t + r.start.x, r.relative_end.y * pre_t + r.start.y, r.relative_end.z * pre_t + r.start.z);

                    if (r.OnWall(pnt, wall.shape.polygon))
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
                if (Vector.ScolarMult(new Point3D(r.hit_wall.shape.polygon.A, r.hit_wall.shape.polygon.B, r.hit_wall.shape.polygon.C), r.light.relative_end) <= 0)
                {
                    r.light = null; 
                    return;
                }

                foreach (Surface wall in m_walls)
                {
                    if (wall.shape.polygon == r.hit_wall.shape.polygon)
                        continue;

                    float pre_t = r.light.FindT(wall.shape.polygon);

                    if (pre_t > 0 && pre_t <= 1)
                    {
                        Point3D pnt = new Point3D(r.light.relative_end.x * pre_t + r.light.start.x, r.light.relative_end.y * pre_t + r.light.start.y, r.light.relative_end.z * pre_t + r.light.start.z);

                        if (r.light.OnWall(pnt, wall.shape.polygon))
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
        public class PolarCoordinate
        {
            public Polygon main_plane;
            public float angle;
            public float distance;
            public Ray3D ray;
            public PolarCoordinate(Ray3D ray)
            {
                main_plane = ray.hit_wall.shape.polygon;
                FindPolarCoordinate(this);
            }

            public void FindPolarCoordinate(PolarCoordinate pol_coord)
            {
                Vector a = new Vector(ray.hit_wall.shape.polygon.vertex1, ray.hit_wall.shape.polygon.vertex2);
                Vector b = new Vector(ray.hit_wall.shape.polygon.vertex1, ray.hit_point);
                Point3D cp = Vector.CrossProduct(a, b);
                Point3D cp1 = new Point3D
                   (cp.x / (a.relative_end.x * b.relative_end.x),
                    cp.y / (a.relative_end.y * b.relative_end.y),
                    cp.z / (a.relative_end.z * b.relative_end.z));
                pol_coord.distance = Vector.Length(b);
                pol_coord.angle = pol_coord.distance / Vector.Length(cp1);// is a cosine
            }
        }

        public void MakeCube(Point3D size, Point3D offset, float reflectivity, Color color) 
        {
           /* 000 011 010 */ m_walls.Add(new Surface(new Shape(new Point3D(offset.x, offset.y, offset.z),                    new Point3D(offset.x, offset.y + size.y, offset.z + size.z),          new Point3D(offset.x, offset.y + size.y, offset.z)),                   new SurfaceParam(color, reflectivity)));
           /* 000 001 011 */ m_walls.Add(new Surface(new Shape(new Point3D(offset.x, offset.y, offset.z),                    new Point3D(offset.x, offset.y, offset.z + size.z),                   new Point3D(offset.x, offset.y + size.y, offset.z + size.z)),          new SurfaceParam(color, reflectivity)));
           /* 001 101 111 */ m_walls.Add(new Surface(new Shape(new Point3D(offset.x, offset.y, offset.z + size.z),           new Point3D(offset.x + size.x, offset.y, offset.z + size.z),          new Point3D(offset.x + size.x, offset.y + size.y, offset.z + size.z)), new SurfaceParam(color, reflectivity)));
           /* 001 111 011 */ m_walls.Add(new Surface(new Shape(new Point3D(offset.x, offset.y, offset.z + size.z),           new Point3D(offset.x + size.x, offset.y + size.y, offset.z + size.z), new Point3D(offset.x, offset.y + size.y, offset.z + size.z)),          new SurfaceParam(color, reflectivity)));
           /* 101 110 111 */ m_walls.Add(new Surface(new Shape(new Point3D(offset.x + size.x, offset.y , offset.z + size.z), new Point3D(offset.x + size.x, offset.y + size.y, offset.z),          new Point3D(offset.x + size.x, offset.y + size.y, offset.z + size.z)), new SurfaceParam(color, reflectivity)));
           /* 101 100 110 */ m_walls.Add(new Surface(new Shape(new Point3D(offset.x + size.x, offset.y, offset.z + size.z),  new Point3D(offset.x + size.x, offset.y, offset.z),                   new Point3D(offset.x + size.x, offset.y + size.y, offset.z)),          new SurfaceParam(color, reflectivity)));
           /* 100 010 110 */ m_walls.Add(new Surface(new Shape(new Point3D(offset.x + size.x, offset.y, offset.z),           new Point3D(offset.x, offset.y + size.y, offset.z),                   new Point3D(offset.x + size.x, offset.y + size.y, offset.z)),          new SurfaceParam(color, reflectivity)));
           /* 100 000 010 */ m_walls.Add(new Surface(new Shape(new Point3D(offset.x + size.x, offset.y, offset.z),           new Point3D(offset.x, offset.y, offset.z),                            new Point3D(offset.x, offset.y + size.y, offset.z)),                   new SurfaceParam(color, reflectivity)));
           /* 000 101 001 */ m_walls.Add(new Surface(new Shape(new Point3D(offset.x, offset.y, offset.z),                    new Point3D(offset.x + size.x, offset.y, offset.z + size.z),          new Point3D(offset.x, offset.y, offset.z + size.z)),                   new SurfaceParam(color, reflectivity)));
           /* 000 100 101 */ m_walls.Add(new Surface(new Shape(new Point3D(offset.x, offset.y, offset.z),                    new Point3D(offset.x + size.x, offset.y, offset.z),                   new Point3D(offset.x + size.x, offset.y, offset.z + size.z)),          new SurfaceParam(color, reflectivity)));
           /* 011 111 010 */ m_walls.Add(new Surface(new Shape(new Point3D(offset.x, offset.y + size.y, offset.z + size.z),  new Point3D(offset.x + size.x, offset.y + size.y, offset.z + size.z), new Point3D(offset.x, offset.y + size.y, offset.z)),                   new SurfaceParam(color, reflectivity)));
           /* 010 111 110 */ m_walls.Add(new Surface(new Shape(new Point3D(offset.x, offset.y + size.y, offset.z),           new Point3D(offset.x + size.x, offset.y + size.y, offset.z + size.z), new Point3D(offset.x + size.x, offset.y + size.y, offset.z)),          new SurfaceParam(color, reflectivity)));
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
