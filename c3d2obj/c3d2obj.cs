using System;
using System.Collections.Generic;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace c3d2obj
{
    class Program
    {
        
        public class cVertex
        {
            public Single Y;
            public Single Z;
            public Single X;
            public Single nY;
            public Single nZ;
            public Single nX;
            public Single U;
            public Single V;
        }

        public class cOutputVertex : IComparable
        {
            public Single X;
            public Single Y;
            public Single Z;

            public int CompareTo(object obj)
            {
                cOutputVertex c = (cOutputVertex)obj;
                if ((this.X == c.X) && (this.Y == c.Y) && (this.Z == c.Z))
                    return 0;
                else
                    return -1;
            }
        }

        class XYZ
        {

            cOutputVertex s;

            public XYZ(cOutputVertex ss)
            {
                s = ss;
            }

            public bool eq(cOutputVertex e)
            {
                if ((s.X == e.X) && (s.Y == e.Y) && (s.Z == e.Z))
                    return true;
                else
                    return false;
            }
        } 

        public class cOutputUV : IComparable
        {
            public Single U;
            public Single V;

            public int CompareTo(object obj)
            {
                cOutputUV c = (cOutputUV)obj;
                if ((this.U == c.U) && (this.V == c.V))
                    return 0;
                else
                    return -1;
            }
        }

        class UV
        {

            cOutputUV s;

            public UV(cOutputUV ss)
            {
                s = ss;
            }

            public bool eq(cOutputUV e)
            {
                if ((s.U == e.U) && (s.V == e.V))
                    return true;
                else
                    return false;
            }
        } 

        public class cOutputNormal : IComparable
        {
            public Single A;
            public Single B;
            public Single C;

            public int CompareTo(object obj)
            {
                cOutputNormal c = (cOutputNormal)obj;
                if ((this.A == c.A) && (this.B == c.B) && (this.C == c.C))
                    return 0;
                else
                    return -1;
            }
        }

        class ABC
        {

            cOutputNormal s;

            public ABC(cOutputNormal ss)
            {
                s = ss;
            }

            public bool eq(cOutputNormal e)
            {
                if ((s.A == e.A) && (s.B == e.B) && (s.C == e.C))
                    return true;
                else
                    return false;
            }
        } 

        public class objParams
        {
            public Single Red;
            public Single Green;
            public Single Blue;
            public Single Alpha;
            public Single Spec;
            public Single Shiny;
            public Single Env;
        }

        public class cFace
        {
            public Int32 A;
            public Int32 B;
            public Int32 C;
        }

        public static int SwapEndianness(int value)
        {
            var b1 = (value >> 0) & 0xff;
            var b2 = (value >> 8) & 0xff;
            var b3 = (value >> 16) & 0xff;
            var b4 = (value >> 24) & 0xff;

            return b1 << 24 | b2 << 16 | b3 << 8 | b4 << 0;
        }

        static private bool isInGeometryList(cVertex v, List<cOutputVertex> OutputVertexList)
        {
            foreach (cOutputVertex ov in OutputVertexList)
            {

                if (v.X != ov.X) continue;
                if (v.Y != ov.Y) continue;
                if (v.Z != ov.Z) continue;
                return true;
            }
            return false;
        }

        static private bool isInNormalList(cVertex v, List<cOutputNormal> OutputNormalList)
        {
            foreach (cOutputNormal ov in OutputNormalList)
            {

                if (v.nX != ov.A) continue;
                if (v.nY != ov.B) continue;
                if (v.nZ != ov.C) continue;
                return true;
            }
            return false;
        }

        static private bool isInUVList(cVertex v, List<cOutputUV> OutputUVList)
        {
            foreach (cOutputUV ov in OutputUVList)
            {

                if (v.U != ov.U) continue;
                if (v.V != ov.V) continue;
                return true;
            }
            return false;
        }

        static int Main(string[] args)
        {

            List<int> NbrVertices_x3 = new List<int>();
            List<int> FaceStartIndex = new List<int>();
            List<int> NbrFaces_x3 = new List<int>();
            List<int> VertexStartIndex = new List<int>();
            List<String> objName = new List<String>();
            List<String> textureName = new List<String>();
            List<objParams> ObjParamList = new List<objParams>();
            List<cOutputVertex> OutputVertexList = new List<cOutputVertex>();
            List<cOutputNormal> OutputNormalList = new List<cOutputNormal>();
            List<cOutputUV> OutputUVList = new List<cOutputUV>();
            
            Common.InputArguments arguments = new Common.InputArguments(args);

            bool verbose = arguments.Contains("v");

            if ( !arguments.Contains("f"))
            {
                Version version = Assembly.GetExecutingAssembly().GetName().Version; 
                Console.WriteLine("Version {0}.{1}", version.Major, version.Minor );
                Console.WriteLine("Usage: c3d2obj -f <filename> | -v");
                return 1;
            }

            string FileBaseName = arguments["-f"];

            try
            {
                string extension = Path.GetExtension(FileBaseName);
                if (extension == null)
                {
                    Console.WriteLine("Usage: c3d2obj -f <filename> | -v");
                    return 1;
                }
                else if (extension == String.Empty)
                {
                    FileBaseName += ".c3d";
                }
                else if (Path.GetExtension(FileBaseName) != ".c3d")
                {
                    Console.WriteLine("Usage: c3d2obj -f <filename> | -v");
                    return 1;
                }

                // Open a filestream to read from
                FileStream fsC3DReader = new FileStream(FileBaseName, FileMode.Open); 

                // Pass read stream to BinaryReader
                BinaryReader readBinary = new BinaryReader(fsC3DReader, System.Text.Encoding.GetEncoding("iso-8859-1"));

                // Create new file name to write to
                string WriteFN = Path.ChangeExtension(FileBaseName, ".obj");
                // Open filestream to write to
                FileStream fsObjWriter = new FileStream(WriteFN, FileMode.Create);
                // Pass write file stream to a StreamWriter
                TextWriter writer = new StreamWriter(fsObjWriter);

                // Create new file name to write material to
                string MtlFN = Path.ChangeExtension(FileBaseName, ".mtl");
                // Open filestream to write to
                FileStream fsMtlWriter = new FileStream(MtlFN, FileMode.Create);
                // Pass write file stream to a StreamWriter
                TextWriter MtlWriter = new StreamWriter(fsMtlWriter);

                int count = 0;
                try
                {
                    if (readBinary.ReadInt32() != 0x01443343)
                    {
                        Console.Write("Not a C3D File");
                        throw new FormatException();
                    }

                    int unknown1 = SwapEndianness(readBinary.ReadInt32());
                    int NbrObj = SwapEndianness(readBinary.ReadInt32());
                    int NbrTextures = SwapEndianness(readBinary.ReadInt32());
                    byte[] x = readBinary.ReadBytes(3);

                    if (verbose) Console.WriteLine("File contains {0} objects", NbrObj);

                    // For each object in the c3d file read each object's generic parameters
                    for (count = 0; count < NbrObj; count++)
                    {
                        // Read the object parameters
                        objParams objParam = new objParams();
                        objName.Add(readBinary.ReadString());
                        VertexStartIndex.Add(readBinary.ReadInt32());
                        NbrVertices_x3.Add(readBinary.ReadInt32());
                        FaceStartIndex.Add(readBinary.ReadInt32());
                        NbrFaces_x3.Add(readBinary.ReadInt32());
                        textureName.Add(readBinary.ReadString());
                        objParam.Red = readBinary.ReadSingle();
                        objParam.Green = readBinary.ReadSingle();
                        objParam.Blue = readBinary.ReadSingle();
                        objParam.Alpha = readBinary.ReadSingle();
                        objParam.Spec = readBinary.ReadSingle();
                        objParam.Shiny = readBinary.ReadSingle();
                        objParam.Env = readBinary.ReadSingle();
                        // Add this object to the list
                        ObjParamList.Add(objParam);

                        // Write material file
                        MtlWriter.Write("# Blender MTL File from c3d2obj" + Environment.NewLine);
                        MtlWriter.Write("newmtl Material_" + count.ToString() + Environment.NewLine);
                        MtlWriter.Write("Ns 300.0" + Environment.NewLine);
                        MtlWriter.Write("Ka 1.000000 1.000000 1.000000" + Environment.NewLine); // Ambient
                        MtlWriter.Write("Kd " + objParam.Red.ToString("0.000000") + " " + objParam.Green.ToString("0.000000") + " " + objParam.Blue.ToString("0.000000") + Environment.NewLine); // Ambient
                        MtlWriter.Write("Ks " + objParam.Spec.ToString("0.000000") + " " + objParam.Spec.ToString("0.000000") + " " + objParam.Spec.ToString("0.000000") + Environment.NewLine); // Ambient
                        MtlWriter.Write("Ni 1.5" + Environment.NewLine);
                        MtlWriter.Write("d 1.000000" + Environment.NewLine);
                        MtlWriter.Write("illum 2" + Environment.NewLine);
                        MtlWriter.Write("map_Kd " + textureName[count] + Environment.NewLine);
                    }
                    if (verbose) Console.WriteLine(MtlFN + " written");

                    // Next is the total number of vertices (over all objects)
                    int TotalNbrVertices = readBinary.ReadInt32();
                    List<cVertex> VertexList = new List<cVertex>();

                    // Read the vertex information into VertexList
                    for (count = 0; count < TotalNbrVertices; count++)
                    {
                        cVertex Vertex = new cVertex();
                        Vertex.Y = readBinary.ReadSingle();
                        Vertex.X = -readBinary.ReadSingle();
                        Vertex.Z = readBinary.ReadSingle();
                        Vertex.nY = readBinary.ReadSingle();
                        Vertex.nX = -readBinary.ReadSingle();
                        Vertex.nZ = readBinary.ReadSingle();
                        Vertex.U = readBinary.ReadSingle();
                        Vertex.V = 1.0f - readBinary.ReadSingle();
                        // Add this vertex to the list
                        VertexList.Add(Vertex);
                    }

                    // Number of faces to end of file (triplets)
                    int nbrFaces = readBinary.ReadInt32();

                    // Read face information
                    List<cFace> FaceList = new List<cFace>();
                    for (int i = 0; i < nbrFaces / 3; i++)
                    {
                        cFace Face = new cFace();
                        Face.A = readBinary.ReadInt32();
                        Face.B = readBinary.ReadInt32();
                        Face.C = readBinary.ReadInt32();
                        FaceList.Add(Face);
                    }

                    // Done reading so close .c3d file
                    fsC3DReader.Close();
                        
                    /*
                    * Now for each object we have the general information such as names and RGB values.
                    * There is also a list of vertices with their X Y Z nX Ny nZ U and V values
                    * For each face there is a list of indices into VertexList
                    * 
                    * Now need to rearrange all that data into .obj format
                    * There is a separate section for each object
                    */
                    int faceIndex = 0;
                    int UVIndex = 0;
                    int OldUVIndex = 0;
                    int XYZIndex = 0;
                    int OldXYZIndex = 0;
                    int NormalIndex = 0;
                    int OldNormalIndex = 0;
                    
                    #region for each object
                    for (int objCount = 0; objCount < NbrObj; objCount++)
                    {
                        // Write name of material file
                        writer.Write("mtllib " + Path.GetFileName(MtlFN) + Environment.NewLine);
                        writer.Write("o " + objName[objCount] + Environment.NewLine);

                        int maxCount;

                        maxCount = NbrVertices_x3[objCount];
                        // Create list of vertices with the same X Y Z values
                        OutputVertexList.Clear();
                        XYZIndex = VertexStartIndex[objCount];
                        for (int i = 0; i < maxCount; i++)
                        {
                            cVertex v = VertexList[XYZIndex++];
                            if (!isInGeometryList(v, OutputVertexList))
                            {
                                // Add current vertex to output list
                                cOutputVertex ov = new cOutputVertex();
                                ov.X = v.X;
                                ov.Y = v.Y;
                                ov.Z = v.Z;
                                // Add this to the OutputVertexList
                                OutputVertexList.Add(ov);
                                // Write v .obj record
                                writer.Write("v " + ov.X.ToString("0.000000") + " " + ov.Z.ToString("0.000000") + " " + ov.Y.ToString("0.000000") + Environment.NewLine);
                            }
                        }

                        OutputUVList.Clear();
                        // Create list of unique UVs
                        UVIndex = VertexStartIndex[objCount];
                        for (int i = 0; i < maxCount; i++)
                        {
                            cVertex v = VertexList[UVIndex++];
                            if (!isInUVList(v, OutputUVList))
                            {
                                // Add current normal to output list
                                cOutputUV oUV = new cOutputUV();
                                oUV.U = v.U;
                                oUV.V = v.V;
                                OutputUVList.Add(oUV);
                                writer.Write("vt " + oUV.U.ToString("0.000000") + " " + oUV.V.ToString("0.000000") + Environment.NewLine);
                            }
                        }

                        // Create list of unique Normals
                        OutputNormalList.Clear();
                        NormalIndex = VertexStartIndex[objCount];
                        for (int i = 0; i < maxCount; i++)
                        {
                            cVertex v = VertexList[NormalIndex++];
                            if (!isInNormalList(v, OutputNormalList))
                            {
                                // Add current normal to output list
                                cOutputNormal oNormal = new cOutputNormal();
                                oNormal.A = v.nX;
                                oNormal.B = v.nY;
                                oNormal.C = v.nZ;
                                OutputNormalList.Add(oNormal);
                                writer.Write("vn " + oNormal.A.ToString("0.000000") + " " + oNormal.C.ToString("0.000000") + " " + oNormal.B.ToString("0.000000") + Environment.NewLine);
                            }
                        }

                        writer.Write("usemtl Material_" + objCount.ToString() + Environment.NewLine);
                        writer.Write("s off" + Environment.NewLine); // No smoothing group


                        int[] vNum = new int[3]; // vertex number
                        int[] tNum = new int[3]; // texture
                        int[] nNum = new int[3]; // Normal

                        /*
                            * for each face we need 9 numbers, which are indecies
                            * i.e. the first vertex is number 1, the second number 2 etc.
                            * The same applies for textures (UV coordinates) and Normals
                            * 
                            * 
                            */
                        faceIndex = FaceStartIndex[objCount] / 3;
                        for (int i = 0; i < NbrFaces_x3[objCount] / 3; i++)
                        {
                            cFace f = FaceList[faceIndex++];
                            // f.A is the integer index of one vertex of the face
                            // Get the XYZ of that vertex
                            var Coord = new cOutputVertex();
                            Coord.X = VertexList[f.A].X;
                            Coord.Y = VertexList[f.A].Y;
                            Coord.Z = VertexList[f.A].Z;
                            var es = new XYZ(Coord);
                            // Now find the index of that coordinate in the OutputVertexList 
                            vNum[0] = OutputVertexList.FindIndex(es.eq);
                            if (vNum[0] == -1)
                                break;
                            vNum[0] += 1 + OldXYZIndex;

                            Coord.X = VertexList[f.B].X;
                            Coord.Y = VertexList[f.B].Y;
                            Coord.Z = VertexList[f.B].Z;
                            // Now find the index of that coordinate in the OutputVertexList 
                            vNum[1] = OutputVertexList.FindIndex(es.eq);
                            if (vNum[1] == -1)
                                break;
                            vNum[1] += 1 + OldXYZIndex;

                            Coord.X = VertexList[f.C].X;
                            Coord.Y = VertexList[f.C].Y;
                            Coord.Z = VertexList[f.C].Z;
                            // Now find the index of that coordinate in the OutputVertexList 
                            vNum[2] = OutputVertexList.FindIndex(es.eq);
                            if (vNum[2] == -1)
                                break;
                            vNum[2] += 1 + OldXYZIndex;

                            var OUV = new cOutputUV();
                            OUV.U = VertexList[f.A].U;
                            OUV.V = VertexList[f.A].V;
                            var oOUV = new UV(OUV);

                            // Now find the index of that coordinate in the OutputUVList 
                            tNum[0] = OutputUVList.FindIndex(oOUV.eq);
                            if (tNum[0] == -1)
                                break;
                            tNum[0] += 1 + OldUVIndex;

                            OUV.U = VertexList[f.B].U;
                            OUV.V = VertexList[f.B].V;
                            // Now find the index of that coordinate in the OutputUVList 
                            tNum[1] = OutputUVList.FindIndex(oOUV.eq);
                            if (tNum[1] == -1)
                                break;
                            tNum[1] += 1 + OldUVIndex;

                            OUV.U = VertexList[f.C].U;
                            OUV.V = VertexList[f.C].V;
                            // Now find the index of that coordinate in the OutputUVList 
                            tNum[2] = OutputUVList.FindIndex(oOUV.eq);
                            if (tNum[2] == -1)
                                break;
                            tNum[2] += 1 + OldUVIndex;

                            var ONorm = new cOutputNormal();
                            ONorm.A = VertexList[f.A].nX;
                            ONorm.B = VertexList[f.A].nY;
                            ONorm.C = VertexList[f.A].nZ;
                            var oABC = new ABC(ONorm);

                            // Now find the index of that coordinate in the OutputNormalList 
                            nNum[0] = OutputNormalList.FindIndex(oABC.eq);
                            if (nNum[0] == -1)
                                break;
                            nNum[0] += 1 + OldNormalIndex;

                            ONorm.A = VertexList[f.B].nX;
                            ONorm.B = VertexList[f.B].nY;
                            ONorm.C = VertexList[f.B].nZ;
                            // Now find the index of that coordinate in the OutputNormalList 
                            nNum[1] = OutputNormalList.FindIndex(oABC.eq);
                            if (nNum[1] == -1)
                                break;
                            nNum[1] += 1 + OldNormalIndex;

                            ONorm.A = VertexList[f.C].nX;
                            ONorm.B = VertexList[f.C].nY;
                            ONorm.C = VertexList[f.C].nZ;
                            // Now find the index of that coordinate in the OutputNormalList 
                            nNum[2] = OutputNormalList.FindIndex(oABC.eq);
                            if (nNum[2] == -1)
                                break;
                            nNum[2] += 1 + OldNormalIndex;

                            // The order these are written is important!
                            writer.Write("f " + vNum[0] + "/" + tNum[0] + "/" + nNum[0] + " ");
                            writer.Write(vNum[2] + "/" + tNum[2] + "/" + nNum[2] + " ");
                            writer.Write(vNum[1] + "/" + tNum[1] + "/" + nNum[1] + Environment.NewLine);
                            writer.Flush();
                        }
                        OldXYZIndex += OutputVertexList.Count;
                        OldUVIndex += OutputUVList.Count;
                        OldNormalIndex += OutputNormalList.Count;
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Oh dear something went wrong");
                    Console.WriteLine(ex.Message);
                    writer.Flush();
                    fsObjWriter.Close();
                    MtlWriter.Flush();
                    fsMtlWriter.Close();
                }
                writer.Flush();
                fsObjWriter.Close();
                MtlWriter.Flush();
                fsMtlWriter.Close();
                if (verbose) Console.WriteLine(WriteFN + " written");
                Console.WriteLine("Done");
            }
            catch (SecurityException ex)
            {
                Console.WriteLine("Security error.\n\nError message: {ex.Message}\n\n" +
                "Details:\n\n{ex.StackTrace}");
            }
            return 0;
        }
    }
}

            
            
          
