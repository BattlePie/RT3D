﻿using System;
using System.Collections.Generic;
using System.Drawing;
namespace Simple3D
{
    public class Point3D
    {
        public float x;
        public float y;
        public float z;
        public Point3D(float x_input, float y_input, float z_input)
        {
            x = x_input;
            y = y_input;
            z = z_input;
        }
    }
    public class Vector
    {

        public Point3D start;
        public Point3D end;
        public Point3D relative_end;
        public Vector(Point3D input_starting_point, Point3D input_end)
        {
            start = input_starting_point;
            end = input_end;
            relative_end = new Point3D(end.x - start.x, end.y - start.y, end.z - start.z);
        }
        public static float Length(Vector input_vector)
        {
            return (float)Math.Sqrt(input_vector.relative_end.x * input_vector.relative_end.x + input_vector.relative_end.y * input_vector.relative_end.y + input_vector.relative_end.z * input_vector.relative_end.z);
        }
        public static float Length(Point3D vector_end_math_result)
        {
            return (float)Math.Sqrt(vector_end_math_result.x * vector_end_math_result.x + vector_end_math_result.y * vector_end_math_result.y + vector_end_math_result.z * vector_end_math_result.z);
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
        public static float ScolarMult(Vector A, Vector B)
        {
            return A.relative_end.x * B.relative_end.x + A.relative_end.y * B.relative_end.y + A.relative_end.z * B.relative_end.z;
        }
        public static float ScolarMult(Point3D A, Point3D B)
        {
            return A.x * B.x + A.y * B.y + A.z * B.z;
        }
        public static Vector Summ(List<Vector> vectors)
        {
            Vector res = new Vector(vectors[0].start, vectors[vectors.Capacity].end);
            return res;
        }
        public static Vector Summ(int n_vectors, Vector[] vectors)
        {
            Vector res = new Vector(vectors[0].start, vectors[n_vectors].end);
            return res;
        }

        public void Normalize()
        {
            float len = Length(this);
            relative_end.x /= len;
            relative_end.y /= len;
            relative_end.z /= len;
        }
        public Point3D FindVectorToPolygonIntersectionPoint(Polygon poly)
        {
            float t = FindT(poly);

                float x = end.x * t;
                float y = end.y * t;
                float z = end.z * t;
            return new Point3D(x, y, z);
        }
        public float FindT(Polygon poly) // Через нормальный вектор и точку
        {
            float div = poly.A * start.x + poly.B * start.y + poly.C * start.z - poly.A * end.x - poly.B * end.y - poly.C * end.z;
            float t;
            if(div != 0)
                t = (poly.A * start.x + poly.B * start.y + poly.C * start.z + poly.D) / div;
            else
                return -1;

            if (t > 0)
                return t;
            else
                return -1;
        }
        public bool OnWall(Point3D m, Polygon poly)
        {
            Vector cl1 = new Vector(poly.vertex1, m);
            Vector wall1 = new Vector(poly.vertex1, poly.vertex2);
            Vector cl2 = new Vector(poly.vertex2, m);
            Vector wall2 = new Vector(poly.vertex2, poly.vertex3);
            Vector cl3 = new Vector(poly.vertex3, m);
            Vector wall3 = new Vector(poly.vertex3, poly.vertex1);
            Point3D Vmult1 = CrossProduct(cl1, wall1);
            Point3D Vmult2 = CrossProduct(cl2, wall2);
            Point3D Vmult3 = CrossProduct(cl3, wall3);
            float Smult1 = ScolarMult(Vmult1, Vmult2);
            float Smult2 = ScolarMult(Vmult2, Vmult3);
            float Smult3 = ScolarMult(Vmult3, Vmult1);

            return Smult1 >= 0 && Smult2 >= 0 && Smult3 >= 0;
        }

    }
    public class Polygon
    {
        public Point3D vertex1;
        public Point3D vertex2;
        public Point3D vertex3;
        public float A;
        public float B;
        public float C;
        public float D;
        public Color color;
        public Polygon(Point3D input_vertex1, Point3D input_vertex2, Point3D input_vertex3, Color input_color)
        {
            vertex1 = input_vertex1;
            vertex2 = input_vertex2;
            vertex3 = input_vertex3;
            color = input_color;
            FindABCD();
        }

        void FindABCD()
        {
            Vector v1 = new Vector(vertex1, vertex2);
            Vector v2 = new Vector(vertex1, vertex3);

            A = v1.relative_end.y * v2.relative_end.z - v1.relative_end.z * v2.relative_end.y;
            B = v1.relative_end.z * v2.relative_end.x - v1.relative_end.x * v2.relative_end.z;
            C = v1.relative_end.x * v2.relative_end.y - v1.relative_end.y * v2.relative_end.x;
            D = (-vertex1.x * A) + (-vertex1.y * B) + (-vertex1.z * C);
        }

    }
}
