using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Simple3D;

namespace RTX3d_test
{
    public partial class Form1 : Form
    {
        //X is forward
        //Y is left
        //Z is up
        Camera cam;
        List<Ray3D> m_rays;
        List<Surface> m_walls;
        int n_rays_x;
        int n_rays_y;
        int screen_width;
        int screen_height;
        int bounces;
        int n_frames;
        float FOV_X;
        float FOV_Y;
        Omnilight m_light;
        static Color ambient_color;

        bool m_enable_room = false;
        bool m_enable_boxes = false;
        bool m_enable_checker_floor = true;
        bool m_enable_sphere = true;

        public Form1()
        {
            InitializeComponent();
            screen_width = ClientRectangle.Width;
            screen_height = ClientRectangle.Height;
            n_rays_x = screen_width;
            n_rays_y = screen_height;
            bounces = 3;
            n_frames = 60;
            FOV_X = 0.5f;
            FOV_Y = 0.5f;
            ambient_color = Color.FromArgb(10, 10, 20);
            m_walls = new List<Surface>();
            m_rays = new List<Ray3D>();
            cam = new Camera(new Point3D(0, 0, 0), new PointF(0, 0));
            //right to left
            //bottom to top

            int floor = -10;
            int ceiling = 200;

            m_light = new Omnilight(new Point3D(105, 0, ceiling / 2), 500, Color.White, 200);

            if (m_enable_room)
            {
                /*Дальняя стена*/
                Surface far_wall1 = new Surface(new Polygon(new Point3D(200, -201, ceiling), new Point3D(201, 200 , floor), new Point3D(201, -200, floor)), new Material(Color.Black,1,300,0));
                Surface far_wall2 = new Surface(new Polygon(new Point3D(200, -201, ceiling), new Point3D(201, 200, ceiling), new Point3D(200, 200, floor)), new Material(Color.Black, 1f, 300, 0f));
                /* Параметры дальней стены */
                {
                    Bitmap normal_map = new Bitmap("D:\\textures\\normalmaptest.bmp");
                    far_wall1.material.normal_map = normal_map;
                    far_wall2.material.normal_map = normal_map;

                    
                    far_wall1.material.uv1 = new PointF(0, 0);
                    far_wall1.material.uv2 = new PointF(far_wall2.material.normal_map.Width, far_wall1.material.normal_map.Height);
                    far_wall1.material.uv3 = new PointF(0, far_wall1.material.normal_map.Height);

                    far_wall2.material.uv1 = new PointF(0, 0);
                    far_wall2.material.uv2 = new PointF(far_wall2.material.normal_map.Width, 0);
                    far_wall2.material.uv3 = new PointF(far_wall2.material.normal_map.Width, far_wall1.material.normal_map.Height);
                    
                    m_walls.Add(far_wall1);
                    m_walls.Add(far_wall2);
                }
                /* Задняя стена*/
                m_walls.Add(new Surface(new Polygon(new Point3D(0, -200, floor), new Point3D(0, 200, floor), new Point3D(0, -200, ceiling)), new Material(Color.Black, 1f, 10000)));
                m_walls.Add(new Surface(new Polygon(new Point3D(0, -200, ceiling), new Point3D(0, 200, floor), new Point3D(0, 200, ceiling)), new Material(Color.Black, 1f, 100000)));
                                            
                /*Правая стена*/            
                m_walls.Add(new Surface(new Polygon(new Point3D(200, 200, floor), new Point3D(200, 200, ceiling), new Point3D(0, 200, ceiling)), new Material(Color.Blue, 0f)));
                m_walls.Add(new Surface(new Polygon(new Point3D(200, 200, floor), new Point3D(0, 200, ceiling), new Point3D(0, 200, floor)), new Material(Color.Blue, 0f)));
                                            
                /* Левая стена*/            
                m_walls.Add(new Surface(new Polygon(new Point3D(200, -200, floor), new Point3D(0, -200, ceiling), new Point3D(200, -200, ceiling)), new Material(Color.Red, 0f)));
                m_walls.Add(new Surface(new Polygon(new Point3D(200, -200, floor), new Point3D(0, -200, floor), new Point3D(0, -200, ceiling)), new Material(Color.Red, 0f)));

                /* Пол*/
                Surface floor1 = new Surface(new Polygon(new Point3D(200, 200, floor), new Point3D(0, 200, floor), new Point3D(201, -200, floor)), new Material(Color.White, 0.1f, 300, 0.1f));
                Surface floor2 = new Surface(new Polygon(new Point3D(0, 200, floor), new Point3D(0, -200, floor), new Point3D(201, -200, floor)), new Material(Color.White, 0.1f, 300, 0.1f));
                /* Параметры пола */
                {
                    Bitmap texture = new Bitmap("D:\\textures\\wooden_floor.bmp");
                    floor1.material.texture = texture;
                    floor2.material.texture = texture;

                    floor1.material.uv1 = new PointF(0, 0);
                    floor1.material.uv2 = new PointF(floor1.material.texture.Width, 0);
                    floor1.material.uv3 = new PointF(0, floor1.material.texture.Height);

                    floor2.material.uv1 = new PointF(floor1.material.texture.Width, 0);
                    floor2.material.uv2 = new PointF(floor1.material.texture.Width, floor1.material.texture.Height);
                    floor2.material.uv3 = new PointF(0, floor1.material.texture.Height);

                    m_walls.Add(floor1);
                    m_walls.Add(floor2);
                }
                /* Потолок*/
                m_walls.Add(new Surface(new Polygon(new Point3D(0, 200, ceiling), new Point3D(200, 200, ceiling), new Point3D(200, -200, ceiling)), new Material(Color.FromArgb(255, 50, 50, 50), 0.6f, 200)));
                m_walls.Add(new Surface(new Polygon(new Point3D(0, 200, ceiling), new Point3D(200, -200, ceiling), new Point3D(0, -200, ceiling)), new Material(Color.FromArgb(255, 50, 50, 50), 0.6f, 200)));
            }
            if (m_enable_boxes)
            {
                MakeCube(new Point3D(10, 30, 40), new Point3D(140, 20, floor), 0f, Color.Yellow);
                MakeCube(new Point3D(10, 30, 40), new Point3D(70, -50, floor), 0f, Color.DarkRed);
                MakeCube(new Point3D(10, 30, 40), new Point3D(140, -50, floor), 0f, Color.DarkGreen);
                MakeCube(new Point3D(10, 30, 40), new Point3D(70, 20, floor), 0f, Color.DarkBlue);
                //MakeCube(new Point3D(20, 40, 10), new Point3D(100, -100, 30), 0f, Color.DarkRed);
            }
            if (m_enable_checker_floor)
            {
                cam.position.x = -180;
                cam.position.y = 100;
                cam.position.z = 150;
                cam.direction = new PointF(0, -0.5f);

                m_light = new Omnilight(new Point3D(0, 0, ceiling / 2), 700, Color.White, 200);
                Surface floor1 = new Surface(new Polygon(new Point3D(200, 200, floor), new Point3D(0, 200, floor), new Point3D(200, 0, floor)), new Material(Color.FromArgb(0,0,100), 0, 1000, 0));
                Surface floor2 = new Surface(new Polygon(new Point3D(0, 200, floor), new Point3D(0, 0, floor), new Point3D(200, 0, floor)), new Material(Color.FromArgb(100, 0, 0), 0, 1000, 0));
                {
                    Bitmap texture1 = new Bitmap("D:\\textures\\checker.jpg");
                    floor1.material.texture = texture1;
                    floor1.material.uv1 = new PointF(0, 0);
                    floor1.material.uv2 = new PointF(floor1.material.texture.Width, 0);
                    floor1.material.uv3 = new PointF(0, floor1.material.texture.Height);

                    floor2.material.texture = texture1;
                    floor2.material.uv1 = new PointF(floor1.material.texture.Width, 0);
                    floor2.material.uv2 = new PointF(floor1.material.texture.Width, floor1.material.texture.Height);
                    floor2.material.uv3 = new PointF(0, floor1.material.texture.Height);
                }
                m_walls.Add(floor1);
                m_walls.Add(floor2);
            }
            if (m_enable_sphere)
            {
                m_walls.Add(new Surface(new Sphere(new Point3D(100, 140, floor + 30), 20), new Material(Color.Yellow, 1f, 300, 0)));
                m_walls.Add(new Surface(new Sphere(new Point3D(100, 60, floor + 30), 20), new Material(Color.Red, 1f, 300, 1)));
            }

            for (int i = 0; i < n_frames; i++)
            {
                m_rays.Clear();
                //cam.Gimball(i * (2f * (float)Math.PI / n_frames), new Point3D(100, 100, 0), 100);

                NewFrame();
                Bitmap frame = FillFrame();
                frame.Save("D://RT_images/res" + i + ".bmp");
            }

        }
        public void NewFrame()
        {
            SetRays();
            FillT();
        }
        public Bitmap FillFrame()
        {
            Bitmap res = new Bitmap(n_rays_x, n_rays_y);

            for (int i = 0; i < n_rays_x; i++)
            {
                for (int j = 0; j < n_rays_y; j++)
                {
                    res.SetPixel(i, j, m_rays[j * n_rays_x + i].CalculateColor());
                }
            }
            return res;
        }
        public class Camera
        {
            public PointF direction;
            public Point3D position;
            public Camera(Point3D position, PointF direction) 
            {
                this.position = position;
                this.direction = direction;
            }
            public void Gimball(float angle, Point3D pivot, float distance) // doesnt work
            {
                position.x = distance * (float)Math.Cos(angle) + pivot.x;
                position.y = distance * (float)Math.Sin(angle) + pivot.y;

                Vector cam_to_pivot = new Vector(pivot, position);
                
                float angle_x = (float)Math.Acos(cam_to_pivot.relative_end.y / cam_to_pivot.Length());
                float angle_y = (float)Math.Acos(cam_to_pivot.relative_end.z / cam_to_pivot.Length());

                if (cam_to_pivot.relative_end.x <= pivot.x)
                { direction.X = angle_x; }
                else
                { direction.X = 2 * (float)Math.PI - angle_x; }

                if (cam_to_pivot.relative_end.z <= pivot.z)
                { direction.Y = 2 * (float)Math.PI - angle_y; }
                else
                { direction.Y = angle_y; }
            }
        }
        public class Ray3D : Vector
        {
            public float t = 1000;
            public Surface hit_surface;
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

                if (reflected_ray != null && reflected_ray.hit_surface != null)
                {
                    reflection_color = reflected_ray.CalculateColor();
                    reflection_distance = Math.Max(0, hit_surface.material.reflection_distance - new Vector(reflected_ray.start, reflected_ray.hit_point).Length()) / hit_surface.material.reflection_distance;
                }
                if (hit_surface != null)
                {
                    Color c = Color.FromArgb(ambient_color.R / 255 * hit_surface.material.color.R,
                                             ambient_color.G / 255 * hit_surface.material.color.G,
                                             ambient_color.B / 255 * hit_surface.material.color.B);
                if (light != null)
                {
                    float light_length = light.Length();
                    float fallof_power = (light.light.power - light_length) / light.light.power;
                    float light_power = Math.Max(0, fallof_power * fallof_power);

                    float p1 = light_power / 255.0f;
                    Color hitwall_color = hit_surface.material.color;

                    if (hit_surface.material.texture != null)
                    {
                            Point uv_coords = hit_surface.shape.CalculateUVcoordinates(hit_point, hit_surface.material.texture, hit_surface.material.uv1, hit_surface.material.uv2, hit_surface.material.uv3);
                            hitwall_color = hit_surface.material.texture.GetPixel(uv_coords.X, uv_coords.Y);
                    } 
                        c = Color.FromArgb(255,
                                           Math.Min(255, (int)(hitwall_color.R * (light.light.color.R * p1 + ambient_color.R / 255f))),
                                           Math.Min(255, (int)(hitwall_color.G * (light.light.color.G * p1 + ambient_color.G / 255f))),
                                           Math.Min(255, (int)(hitwall_color.B * (light.light.color.B * p1 + ambient_color.B / 255f))));
                }

                    float reflection_coefficient = hit_surface.material.reflectivity * reflection_distance;

                    float metallic_r = 1 - hit_surface.material.metalness * (1 - hit_surface.material.color.R / 255f);
                    float metallic_g = 1 - hit_surface.material.metalness * (1 - hit_surface.material.color.G / 255f);
                    float metallic_b = 1 - hit_surface.material.metalness * (1 - hit_surface.material.color.B / 255f);
                    return Color.FromArgb(255,
                                          Math.Min(255, (int)(c.R + reflection_color.R * reflection_coefficient * metallic_r)),
                                          Math.Min(255, (int)(c.G + reflection_color.G * reflection_coefficient * metallic_g)),
                                          Math.Min(255, (int)(c.B + reflection_color.B * reflection_coefficient * metallic_b)));
                }
                return Color.FromArgb(255, 0, 0, 0);
            }
            public void CalcReflection()
            {
                Vector n;
                if (hit_surface.material.normal_map != null)
                {
                    n = hit_surface.shape.CalculateNormalMappedCoordinates(hit_point, hit_surface.material.normal_map, hit_surface.material.uv1, hit_surface.material.uv2, hit_surface.material.uv3);
                }
                else
                {
                    n = hit_surface.shape.FindN(hit_point);
                }
                float sm = ScolarMult(this, n);
                Point3D denum = new Point3D(n.relative_end.x * 2 * sm, n.relative_end.y * 2 * sm, n.relative_end.z * 2 * sm);
                reflected_ray = new Ray3D(hit_point,
                    new Point3D(relative_end.x - denum.x + hit_point.x,
                    relative_end.y - denum.y + hit_point.y,
                    relative_end.z - denum.z + hit_point.z));
            }
        }
        public class Surface
        {
            public Material material;
            public Shape shape;
            public Surface(Shape shape, Material material)
            {
                this.material = material;
                this.shape = shape;
            }
        }
        public class Material
        {
            public float reflectivity;
            public float reflection_distance;
            public float metalness;
            public Color color;
            public Bitmap texture;
            public Bitmap normal_map;
            public PointF uv1;
            public PointF uv2;
            public PointF uv3;
            public Material(Color color, float reflectivity = 0f, float reflection_distance = 300f, float metalness = 0.1f)
            {
                this.reflectivity = reflectivity;
                this.reflection_distance = reflection_distance;
                this.color = color;
                this.metalness = metalness;
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
                    {
                        float v_x = (2.0f * i / n_rays_x - 1) * FOV_X;
                        float v_y = (2.0f * j / n_rays_y - 1) * FOV_Y;
                        Ray3D r = new Ray3D(cam.position,
                            new Point3D(cam.position.x + ((float)(Math.Sin(cam.direction.X) * Math.Tan(v_x) + Math.Cos(cam.direction.X))),
                                        cam.position.y + ((float)(Math.Sin(cam.direction.X) - Math.Cos(cam.direction.X) * Math.Tan(v_x))),
                                        cam.position.z + ((float)(Math.Sin(cam.direction.Y) - Math.Cos(cam.direction.Y) * Math.Tan(v_y)))));
                        m_rays.Add(r);
                    }
                }
            }

        }
        public void FillT()
        {
            foreach (Ray3D r in m_rays)
            {
                TestRay(r);
                if(r.hit_surface != null)
                {
                    Ray3D rr = r;
                    for (int i = 0; i < bounces && rr.hit_surface != null; i++)
                    {
                        rr.CalcReflection();
                        TestRay(rr.reflected_ray, rr.hit_surface);
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

                float pre_t = wall.shape.FindT(r);
                if (pre_t < r.t && pre_t > 0)
                {
                    Vector new_ray = new Vector(r.start, new Point3D(r.relative_end.x * pre_t + r.start.x, r.relative_end.y * pre_t + r.start.y, r.relative_end.z * pre_t + r.start.z));
                    if (wall.shape.OnSurface(new_ray))
                    {
                        r.hit_surface = wall;
                        r.t = pre_t;
                        r.hit_point = new_ray.end;
                    }
                }
            }

            if (r.hit_surface != null)
            {
                r.light = new Lightray(r.hit_point, m_light.pos);
                if (Vector.ScolarMult(r.hit_surface.shape.FindN(r.hit_point).relative_end, r.light.relative_end) <= 0)
                {
                    r.light = null; 
                    return;
                }

                foreach (Surface wall in m_walls)
                {
                    if (wall.shape == r.hit_surface.shape)
                        continue;

                    float pre_t = wall.shape.FindT(r.light);

                    if (pre_t > 0 && pre_t <= 1)
                    {
                        Vector new_light_ray = new Vector(r.light.start, new Point3D(r.light.relative_end.x * pre_t + r.light.start.x, r.light.relative_end.y * pre_t + r.light.start.y, r.light.relative_end.z * pre_t + r.light.start.z));
                        if (wall.shape.OnSurface(new_light_ray))
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
            e.Graphics.DrawImage(FillFrame(),0,0);
        }
        public void MakeCube(Point3D size, Point3D offset, float reflectivity, Color color) 
        {
           /* 000 011 010 */ m_walls.Add(new Surface(new Polygon(new Point3D(offset.x, offset.y, offset.z),                    new Point3D(offset.x, offset.y + size.y, offset.z + size.z),          new Point3D(offset.x, offset.y + size.y, offset.z)),                   new Material(color, reflectivity)));
           /* 000 001 011 */ m_walls.Add(new Surface(new Polygon(new Point3D(offset.x, offset.y, offset.z),                    new Point3D(offset.x, offset.y, offset.z + size.z),                   new Point3D(offset.x, offset.y + size.y, offset.z + size.z)),          new Material(color, reflectivity)));
           /* 001 101 111 */ m_walls.Add(new Surface(new Polygon(new Point3D(offset.x, offset.y, offset.z + size.z),           new Point3D(offset.x + size.x, offset.y, offset.z + size.z),          new Point3D(offset.x + size.x, offset.y + size.y, offset.z + size.z)), new Material(color, reflectivity)));
           /* 001 111 011 */ m_walls.Add(new Surface(new Polygon(new Point3D(offset.x, offset.y, offset.z + size.z),           new Point3D(offset.x + size.x, offset.y + size.y, offset.z + size.z), new Point3D(offset.x, offset.y + size.y, offset.z + size.z)),          new Material(color, reflectivity)));
           /* 101 110 111 */ m_walls.Add(new Surface(new Polygon(new Point3D(offset.x + size.x, offset.y , offset.z + size.z), new Point3D(offset.x + size.x, offset.y + size.y, offset.z),          new Point3D(offset.x + size.x, offset.y + size.y, offset.z + size.z)), new Material(color, reflectivity)));
           /* 101 100 110 */ m_walls.Add(new Surface(new Polygon(new Point3D(offset.x + size.x, offset.y, offset.z + size.z),  new Point3D(offset.x + size.x, offset.y, offset.z),                   new Point3D(offset.x + size.x, offset.y + size.y, offset.z)),          new Material(color, reflectivity)));
           /* 100 010 110 */ m_walls.Add(new Surface(new Polygon(new Point3D(offset.x + size.x, offset.y, offset.z),           new Point3D(offset.x, offset.y + size.y, offset.z),                   new Point3D(offset.x + size.x, offset.y + size.y, offset.z)),          new Material(color, reflectivity)));
           /* 100 000 010 */ m_walls.Add(new Surface(new Polygon(new Point3D(offset.x + size.x, offset.y, offset.z),           new Point3D(offset.x, offset.y, offset.z),                            new Point3D(offset.x, offset.y + size.y, offset.z)),                   new Material(color, reflectivity)));
           /* 000 101 001 */ m_walls.Add(new Surface(new Polygon(new Point3D(offset.x, offset.y, offset.z),                    new Point3D(offset.x + size.x, offset.y, offset.z + size.z),          new Point3D(offset.x, offset.y, offset.z + size.z)),                   new Material(color, reflectivity)));
           /* 000 100 101 */ m_walls.Add(new Surface(new Polygon(new Point3D(offset.x, offset.y, offset.z),                    new Point3D(offset.x + size.x, offset.y, offset.z),                   new Point3D(offset.x + size.x, offset.y, offset.z + size.z)),          new Material(color, reflectivity)));
           /* 011 111 010 */ m_walls.Add(new Surface(new Polygon(new Point3D(offset.x, offset.y + size.y, offset.z + size.z),  new Point3D(offset.x + size.x, offset.y + size.y, offset.z + size.z), new Point3D(offset.x, offset.y + size.y, offset.z)),                   new Material(color, reflectivity)));
           /* 010 111 110 */ m_walls.Add(new Surface(new Polygon(new Point3D(offset.x, offset.y + size.y, offset.z),           new Point3D(offset.x + size.x, offset.y + size.y, offset.z + size.z), new Point3D(offset.x + size.x, offset.y + size.y, offset.z)),          new Material(color, reflectivity)));
        }
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
             Ray3D r = m_rays[(screen_height - e.Y) * n_rays_x + e.X];
            r.hit_surface = null;
            r.t = 10000;
            TestRay(r);
            if (r.hit_surface != null)
            {
                Ray3D rr = r;
                for (int i = 0; i < bounces && rr.hit_surface != null; i++)
                {
                    rr.CalcReflection();
                    TestRay(rr.reflected_ray, rr.hit_surface);
                    rr = rr.reflected_ray;
                }
            }
            r.CalculateColor();
        }
    }
}
