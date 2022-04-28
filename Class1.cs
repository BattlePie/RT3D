using System;
using System.Collections.Generic;
using System.Drawing;
namespace Simple3D
{
    public class Point3D
    {
        public float x;
        public float y;
        public float z;
        public Point3D(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
    public class Vector
    {
        public Point3D start;
        public Point3D end;
        public Point3D relative_end;
        public Vector(Point3D start, Point3D end)
        {
            this.start = start;
            this.end = end;
            relative_end = new Point3D(end.x - start.x, end.y - start.y, end.z - start.z);
        }
        public float Length()
        {
            return (float)Math.Sqrt(relative_end.x * relative_end.x + relative_end.y * relative_end.y + relative_end.z * relative_end.z);
        }
        public static Point3D CrossProduct(Vector A, Vector B)
        {
            float x = A.relative_end.z * B.relative_end.y - A.relative_end.y * B.relative_end.z;
            float y = A.relative_end.x * B.relative_end.z - A.relative_end.z * B.relative_end.x;
            float z = A.relative_end.y * B.relative_end.x - A.relative_end.x * B.relative_end.y;
            Point3D res = new Point3D(x, y, z);
            return res;
        }
        public static Point3D CrossProduct(Point3D A, Point3D B)
        {
            float x = A.z * B.y - A.y * B.z;
            float y = A.x * B.z - A.z * B.x;
            float z = A.y * B.x - A.x * B.y;
            Point3D res = new Point3D(x, y, z);
            return res;
        }
        public float ScalarMult(Vector A, Vector B)
        {
            return A.relative_end.x * B.relative_end.x + A.relative_end.y * B.relative_end.y + A.relative_end.z * B.relative_end.z;
        }
        public static float ScalarMult(Point3D A, Point3D B)
        {
            return A.x * B.x + A.y * B.y + A.z * B.z;
        }
        public void Normalize()
        {
            float len = Length();
            relative_end.x /= len;
            relative_end.y /= len;
            relative_end.z /= len;
        }
        public Vector Normalize(Vector a)
        {
            float len = a.Length();
            a.relative_end.x /= len;
            a.relative_end.y /= len;
            a.relative_end.z /= len;
            return a;
        }
        public float Angle(Vector A, Vector B)
        {
            return 1f / ((float)Math.Acos(ScalarMult(A, B) / (A.Length() * B.Length())));
        }
        public Vector Reflect(Vector n)
        {
            float sm = ScalarMult(this, n);
            Point3D denum = new Point3D(n.relative_end.x * 2 * sm, n.relative_end.y * 2 * sm, n.relative_end.z * 2 * sm);
            return new Vector(end,
            new Point3D(relative_end.x - denum.x + end.x,
            relative_end.y - denum.y + end.y,
            relative_end.z - denum.z + end.z));

        }
        public static Vector operator -(Vector A)
        => new Vector(A.end, A.start);
    }
    public class Shape
    {
        public Shape()
        {
        }
        public virtual Vector FindN(Point3D input)
        {
            return null;
        }
        public virtual float FindT(Vector input)
        {
            return 0f;
        }
        public virtual bool OnSurface(Vector input)
        {
            return false;
        }
        public virtual Point CalculateUVcoordinates(Point3D hit_point,Bitmap texture, PointF corner1, PointF corner2, PointF corner3)
        {
            return new Point(0,0);
        }
        public virtual Vector CalculateNormalMappedCoordinates(Point3D hit_point, Bitmap normal_map, PointF corner1, PointF corner2, PointF corner3)
        {
            return null;
        }
    }
    public class Polygon : Shape
    {
        public Point3D vertex1;
        public Point3D vertex2;
        public Point3D vertex3;
        public float A;
        public float B;
        public float C;
        public float D;
        public Vector n;
        public Polygon(Point3D vertex1, Point3D vertex2, Point3D vertex3)
        {
            this.vertex1 = vertex1;
            this.vertex2 = vertex2;
            this.vertex3 = vertex3;
            FindABCD();
        }
        void FindABCD()
        {
            Vector v1 = new Vector(vertex1, vertex2);
            Vector v2 = new Vector(vertex1, vertex3);

            A = v1.relative_end.y * v2.relative_end.z - v1.relative_end.z * v2.relative_end.y;
            B = v1.relative_end.z * v2.relative_end.x - v1.relative_end.x * v2.relative_end.z;
            C = v1.relative_end.x * v2.relative_end.y - v1.relative_end.y * v2.relative_end.x;
            D = -vertex1.x * A - vertex1.y * B - vertex1.z * C;
            n = new Vector(new Point3D(0, 0, 0), new Point3D(A, B, C));
            n.Normalize();
            
        }
        public override Vector FindN(Point3D input)
        {
            return n;
        }
        public override float FindT(Vector input)
        {
            float div = A * (input.start.x - input.end.x) + B * (input.start.y - input.end.y) + C * (input.start.z - input.end.z);
            float t;
            if (div != 0)
                t = (A * input.start.x + B * input.start.y + C * input.start.z + D) / div;
            else
                return -1;
            if (t > 0)
                return t;
            else
                return -1;
        }
        public override bool OnSurface(Vector input)
        {
            Vector cl1 = new Vector(vertex1, input.end);
            Vector wall1 = new Vector(vertex1, vertex2);
            Vector cl2 = new Vector(vertex2, input.end);
            Vector wall2 = new Vector(vertex2, vertex3);
            Vector cl3 = new Vector(vertex3, input.end);
            Vector wall3 = new Vector(vertex3, vertex1);
            Point3D Vmult1 = Vector.CrossProduct(cl1, wall1);
            Point3D Vmult2 = Vector.CrossProduct(cl2, wall2);
            Point3D Vmult3 = Vector.CrossProduct(cl3, wall3);
            float s1 = Vector.ScalarMult(Vmult1, Vmult2);
            float s2 = Vector.ScalarMult(Vmult2, Vmult3);

            return s1 >= 0 && s2 >= 0;
        }
        public override Point CalculateUVcoordinates(Point3D hit_point, Bitmap texture, PointF corner1, PointF corner2, PointF corner3)
        {
            Point3D A = vertex1;
            Point3D B = vertex2;
            Point3D C = vertex3;

            float denum = B.y * A.x - B.y * C.x - C.y * A.x + C.y * C.x + (C.x - B.x) * (A.y - C.y);
            float BaryA = ((B.y - C.y) * (hit_point.x - C.x) + (C.x - B.x) * (hit_point.y - C.y)) / denum;
            float BaryB = ((C.y - A.y) * (hit_point.x - C.x) + (A.x - C.x) * (hit_point.y - C.y)) / denum;
            float BaryC = 1 - BaryA - BaryB;

            PointF Auv = corner1;
            PointF Buv = corner2;
            PointF Cuv = corner3;

            int u = Math.Abs((int)(BaryA * Auv.X + BaryB * Buv.X + BaryC * Cuv.X)) % texture.Width;
            int v = Math.Abs((int)(BaryA * Auv.Y + BaryB * Buv.Y + BaryC * Cuv.Y)) % texture.Height;

            return new Point(u, v);
        }
        public override Vector CalculateNormalMappedCoordinates(Point3D hit_point, Bitmap normal_map, PointF corner1, PointF corner2, PointF corner3)
        {
            Point UV = CalculateUVcoordinates(hit_point, normal_map, corner1, corner2, corner3);
            Color test_normal = normal_map.GetPixel(UV.X, UV.Y);
            Point3D new_normal = new Point3D(
                                                (test_normal.R / 256f - 0.5f),
                                                (test_normal.G / 256f - 0.5f),
                                                (test_normal.B / 256f - 0.5f));
            Vector X = new Vector(vertex1, vertex2);
            X.Normalize();
            Vector Y = new Vector(new Point3D(0, 0, 0), Vector.CrossProduct(n, X));
            Y.Normalize();

            Vector tmp = new Vector(new Point3D(0, 0, 0), new Point3D(
            new_normal.x * X.relative_end.x + new_normal.y * Y.relative_end.x + new_normal.z * n.relative_end.x,
            new_normal.x * X.relative_end.y + new_normal.y * Y.relative_end.y + new_normal.z * n.relative_end.y,
            new_normal.x * X.relative_end.z + new_normal.y * Y.relative_end.z + new_normal.z * n.relative_end.z));
            tmp.Normalize();
            return tmp;

        }
    }
    public class Sphere : Shape
    {
        public Point3D center;
        public int radius;
        public Sphere(Point3D center, int radius)
        {
            this.center = center;
            this.radius = radius;
        }
        public override Vector FindN(Point3D input)
        {
            Vector n = new Vector(center, input);
            n.Normalize();
            return n;
        }
        public override float FindT(Vector line)
        {
            float a = line.relative_end.x * line.relative_end.x + line.relative_end.y * line.relative_end.y + line.relative_end.z * line.relative_end.z;
            float b = 2 * (line.relative_end.x * (line.start.x - center.x) + line.relative_end.y * (line.start.y - center.y) + line.relative_end.z * (line.start.z - center.z));
            float c = (line.start.x - center.x) * (line.start.x - center.x) + (line.start.y - center.y) * (line.start.y - center.y) + (line.start.z - center.z) * (line.start.z - center.z) - radius * radius;
            float Discriminant = b * b - 4 * a * c;
            float x1 = (-b - (float)Math.Sqrt(Discriminant)) / (2 * a);
            float x2 = (-b + (float)Math.Sqrt(Discriminant)) / (2 * a);
            if (x1 >= 0)
            {
                return x1; 
            }
            else
            {
                return x2;
            }
        }
        public override bool OnSurface(Vector line)
        {
            float a = line.relative_end.x * line.relative_end.x + line.relative_end.y * line.relative_end.y + line.relative_end.z * line.relative_end.z;
            float b = 2 * (line.relative_end.x * (line.start.x - center.x) + line.relative_end.y * (line.start.y - center.y) + line.relative_end.z * (line.start.z - center.z));
            float c = (line.start.x - center.x) * (line.start.x - center.x) + (line.start.y - center.y) * (line.start.y - center.y) + (line.start.z - center.z) * (line.start.z - center.z) - radius * radius;
            float Discriminant = b * b - 4 * a * c;
            return Discriminant >= 0;
        }
        public override Point CalculateUVcoordinates(Point3D hit_point, Bitmap texture, PointF corner1, PointF corner2, PointF corner3)
        {
            throw new NotImplementedException();
        }
        public override Vector CalculateNormalMappedCoordinates(Point3D hit_point, Bitmap normal_map, PointF corner1, PointF corner2, PointF corner3)
        {
            throw new NotImplementedException();
        }
    }
    public class Cylinder : Shape
    {
        public Point3D center;
        public int radius;
        public Cylinder(Point3D center, int radius)
        {
            this.center = center;
            this.radius = radius;
        }
    }
}
