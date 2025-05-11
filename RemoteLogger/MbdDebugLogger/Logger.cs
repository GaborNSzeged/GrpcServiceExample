using System.Collections.Generic;
using CenterSpace.NMath.Core;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using LayeringAnalysis.DTO.Binarized;
using Semilab.MBD.MBDCommon.CoreFeatures.SectionLayering;
using MBD.DTO;
using Semilab.SAM.MBDCommon.Classes;
using Semilab.MBD.MBDCommon.CoreFeatures.SpectrumCalculator;
using Semilab.MBD.MBDCommon.CoreFeatures.SectionBuilder;
using Semilab.MBD.MBDCommon.Interfaces;

namespace MbdDebugLogger
{
    public class Logger
    {
        private const string DirectoryPath = @"c:\MBD_temp\LogNew";
        private static readonly Dictionary<string, int> Counter2FilePath = new Dictionary<string, int>();

        public static void Print(double value, string bcdOld)
        {
            string filePath = Path.Combine(DirectoryPath, $"{bcdOld}.txt");
            using (var stream = File.AppendText(filePath))
            {
                stream.WriteLine(value.ToString("G"));
            }
        }

        // TODO Circular Dependency, create a proxy project which can accept object an delegate the calls to this one and the specific
        // project can cast it to the required object.
        //public static void Print(ISectionCard layeringStructureModel, string extraMarker)
        //{
        //    string ending = string.IsNullOrEmpty(extraMarker) ? string.Empty : $"_{extraMarker}";
        //    string filePath = Path.Combine(DirectoryPath, $"SectionCard{ending}.txt");
        //    bool appendLine = File.Exists(filePath);
        //    int counter = GetNextCount(filePath, appendLine);

        //    var sb = new StringBuilder();
        //    sb.AppendLine($"Index: {counter}");
        //    sb.Append("Positioning.OffsetX: ");
        //    sb.AppendLine(layeringStructureModel.Positioning.OffsetX.EvaluatedValue.ToString("G"));
        //    sb.Append("Parametrization.Alias: ");
        //    sb.AppendLine(layeringStructureModel.Parametrization.Alias);

        //    foreach (IEvaluatedValidableString<double> parametrizationStructureParameter in layeringStructureModel.Parametrization.StructureParameters)
        //    {
        //        sb.AppendLine(parametrizationStructureParameter.EvaluatedValue.ToString("G"));
        //        sb.AppendLine(parametrizationStructureParameter.ValidableString.Name);
        //        sb.AppendLine(parametrizationStructureParameter.ValidableString.Value);
        //    }

        //    Save(sb, filePath, appendLine);
        //}

        public static void Print(DoubleComplexMatrix doubleComplexMatrix, string name, double waveLength, string extraMarker = "")
        {
            string ending = string.IsNullOrEmpty(extraMarker) ? string.Empty : $"_{extraMarker}";
            string filePath = Path.Combine(DirectoryPath, $"DubleComplexMatrix_{name}{ending}.txt");
            bool appendLine = File.Exists(filePath);

            var sb = new StringBuilder();
            sb.AppendLine($"Wavelength: {waveLength}");

            int index = 0;
            foreach (DoubleComplex doubleComplex in doubleComplexMatrix.DataBlock.Data)
            {
                sb.AppendLine($"{index}: Real: {doubleComplex.Real}  Imag: {doubleComplex.Imag}");
                index++;
            }

            Save(sb, filePath, appendLine);
        }

        public static void Print(DoubleComplexVector doubleComplexMatrix, string name, double waveLength, string extraMarker = "")
        {
            string ending = string.IsNullOrEmpty(extraMarker) ? string.Empty : $"_{extraMarker}";
            string filePath = Path.Combine(DirectoryPath, $"DubleComplexVector_{name}{ending}.txt");
            bool appendLine = File.Exists(filePath);

            var sb = new StringBuilder();
            int index = 0;
            sb.AppendLine($"Wavelength: {waveLength}");

            foreach (DoubleComplex doubleComplex in doubleComplexMatrix.DataBlock.Data)
            {
                sb.AppendLine($"{index}: Real: {doubleComplex.Real}  Imag: {doubleComplex.Imag}");
                index++;
            }

            Save(sb, filePath, appendLine);
        }

        public static void Print(ISectionLayer[] sectionLayers, string extraMarker = "")
        {
            string ending = string.IsNullOrEmpty(extraMarker) ? string.Empty : $"_{extraMarker}";
            string filePath = Path.Combine(DirectoryPath, $"SectionLayer_{ending}.txt");
            bool appendLine = File.Exists(filePath);
            int counter = GetNextCount(filePath, appendLine);

            int layerIndex = 0;
            var sb = new StringBuilder();

            foreach (ISectionLayer sectionLayer in sectionLayers)
            {
                sb.AppendLine($"Layer index: {layerIndex++} - LayerArray counter: {counter}");

                sb.Append("LayerEndX: ");
                sb.AppendLine(sectionLayer.LayerEndX.ToString("G"));

                int cellIndex = 0;
                foreach (ILayerCell layerCell in sectionLayer.LayerCells)
                {
                    sb.AppendLine($"Cell index: {cellIndex++}");

                    sb.Append("Cell startX:");
                    sb.AppendLine(layerCell.StartX.ToString("G"));
                    sb.Append("Cell endX: ");
                    sb.AppendLine(layerCell.EndX.ToString("G"));
                    sb.Append("Cell width: ");
                    sb.AppendLine(layerCell.Width.ToString("G"));
                }
            }

            Save(sb, filePath, appendLine);
        }

        public static void Print(List<double> doubleComplexMatrix, string extraMarker)
        {
            string filePath = Path.Combine(DirectoryPath, $"{extraMarker}.txt");
            using (var stream = File.AppendText(filePath))
            {
                var enumerable = doubleComplexMatrix.Select(d => d.ToString("G") + " ");
                var line = string.Join(";", enumerable);
                stream.WriteLine(line);
            }
        }

        public static void Print(Structure layeringStructureModel, string fileName)
        {
            string filePath = Path.Combine(DirectoryPath, $"{fileName}.txt");
            bool appendLine = File.Exists(filePath);
            int doubleCounter = GetNextCount(filePath, appendLine);

            var sb = new StringBuilder();
            sb.AppendLine($"Index: {doubleCounter}");

            sb.Append("SubstrateMaterialId: ");
            sb.AppendLine(layeringStructureModel.SubstrateMaterialId);

            sb.Append("SurroundingMaterialId: ");
            sb.AppendLine(layeringStructureModel.SurroundingMaterialId);

            sb.Append("Pitch: ");
            sb.AppendLine(layeringStructureModel.Pitch.ToString("G"));

            int index = 0;
            foreach (Section section in layeringStructureModel.Sections)
            {
                sb.AppendLine($"Section index: {index++}");
                sb.Append("MaterialId: ");
                sb.AppendLine(section.MaterialId);
                sb.Append("StartX: ");
                sb.AppendLine(section.StartX.ToString("G"));
                sb.Append("StartZ: ");
                sb.AppendLine(section.StartZ.ToString("G"));
                sb.Append("EndX: ");
                sb.AppendLine(section.EndX.ToString("G"));
                sb.Append("EndZ: ");
                sb.AppendLine(section.EndZ.ToString("G"));
                sb.Append("EndZ: ");
                if (section.WidthZ != null)
                {
                    sb.Append("WidthZ: ");
                    sb.AppendLine(section.WidthZ.ToString());
                }
            }

            Save(sb, filePath, appendLine);
        }

        public static void Print(BinarizedStructure layeringStructureModel, string extraMarker)
        {
            string ending = string.IsNullOrEmpty(extraMarker) ? string.Empty : $"_{extraMarker}";
            string filePath = Path.Combine(DirectoryPath, $"BinarizedStructure{ending}.txt");
            bool appendLine = File.Exists(filePath);
            int counter = GetNextCount(filePath, appendLine);

            var sb = new StringBuilder();
            sb.AppendLine($"index: {counter}");
            sb.AppendLine("** Rows **");
            int rowCounter = 0;
            foreach (BinarizedRow binarizedRow in layeringStructureModel.Rows)
            {
                sb.AppendLine($"index: {rowCounter++}");
                sb.Append("StartZ: ");
                sb.AppendLine(binarizedRow.StartZ.ToString("G"));
                sb.Append("EndZ: ");
                sb.AppendLine(binarizedRow.EndZ.ToString("G"));

                int cellCounter = 0;
                sb.AppendLine("* Cells *");
                foreach (BinarizedRowCell binarizedRowCell in binarizedRow.Cells)
                {
                    sb.AppendLine($" index: {cellCounter++}");
                    sb.Append(" MaterialId: ");
                    sb.AppendLine(binarizedRowCell.MaterialId);
                    sb.Append(" Priority: ");
                    sb.AppendLine(binarizedRowCell.Priority.ToString());
                    sb.Append(" SegmentX start: ");
                    sb.AppendLine(binarizedRowCell.SegmentX.Start.ToString("G"));
                    sb.Append(" SegmentX end: ");
                    sb.AppendLine(binarizedRowCell.SegmentX.End.ToString("G"));
                }
            }

            Save(sb, filePath, appendLine);
        }

        public static void Print(Dictionary<UserParameter, double> userParameters, string extraMarker = "")
        {
            string ending = string.IsNullOrEmpty(extraMarker) ? string.Empty : $"_{extraMarker}";
            string filePath = Path.Combine(DirectoryPath, $"UserParamters{ending}.txt");
            bool appendLine = File.Exists(filePath);
            int counter = GetNextCount(filePath, appendLine);

            var sb = new StringBuilder();

            sb.AppendLine($"Index: {counter}");

            foreach (KeyValuePair<UserParameter, double> userParameter in userParameters)
            {
                sb.Append($"{userParameter.Key}: ");
                sb.AppendLine(userParameter.Value.ToString("G"));
            }

            Save(sb, filePath, appendLine);
        }

        public static void Print(ISpectrumCalculatorEvaluatedDataDto layeringStructureModel, string extraMarker)
        {
            string ending = string.IsNullOrEmpty(extraMarker) ? string.Empty : $"_{extraMarker}";
            string filePath = Path.Combine(DirectoryPath, $"SpectrumCalculatorEvaluatedDataDto{ending}.txt");
            bool appendLine = File.Exists(filePath);
            int counter = GetNextCount(filePath, appendLine);

            var sb = new StringBuilder();
            sb.AppendLine($"Index: {counter}");
            sb.Append("AngleOfIncidence: ");
            sb.AppendLine(layeringStructureModel.AngleOfIncidence.ToString("G"));
            sb.Append("AngleOfIncidenceShift: ");
            sb.AppendLine(layeringStructureModel.AngleOfIncidenceShift.ToString("G"));
            sb.Append("AzimuthalAngleOfIncidence: ");
            sb.AppendLine(layeringStructureModel.AzimuthalAngleOfIncidence.ToString("G"));
            sb.Append("MaximumWavelength: ");
            sb.AppendLine(layeringStructureModel.MaximumWavelength.ToString("G"));
            sb.Append("MinimumWavelength: ");
            sb.AppendLine(layeringStructureModel.MinimumWavelength.ToString("G"));
            sb.Append("NumberOfOrders: ");
            sb.AppendLine(layeringStructureModel.NumberOfOrders.ToString("G"));

            var enumerable = layeringStructureModel.Wavelengths.Select(d => d.ToString("G") + " ");
            var line = string.Join(";", enumerable);
            sb.Append("Wavelength: ");
            sb.AppendLine(line);

            Save(sb, filePath, appendLine);
        }


        public static void Print(IStructureModel model, string extraMarker)
        {
            string ending = string.IsNullOrEmpty(extraMarker) ? string.Empty : $"_{extraMarker}";
            string filePath = Path.Combine(DirectoryPath, $"StructureModel{ending}.txt");
            bool appendLine = File.Exists(filePath);
            int counter = GetNextCount(filePath, appendLine);

            var sb = new StringBuilder();
            sb.AppendLine($"Index: {counter}");
            sb.Append("Period: ");
            sb.AppendLine(model.Period.ToString("G"));

            foreach (IPredefinedSection predefinedSection in model.SectionModels)
            {
                sb.Append("WidthZ: ");
                sb.AppendLine(predefinedSection.WidthZ(0).ToString("G"));

                PredefinedSectionBase sectionBase = predefinedSection as PredefinedSectionBase;
                if (sectionBase != null)
                {
                    sb.Append("MaterialAlias: ");
                    sb.AppendLine(sectionBase.MaterialAlias);
                    sb.Append("StartX: ");
                    sb.AppendLine(sectionBase.StartX.ToString("G"));
                    sb.Append("EndX: ");
                    sb.AppendLine(sectionBase.EndX.ToString("G"));
                    sb.Append("StartZ: ");
                    sb.AppendLine(sectionBase.StartZ.ToString("G"));
                    sb.Append("EndZ: ");
                    sb.AppendLine(sectionBase.EndZ.ToString("G"));
                    sb.Append("OffsetX: ");
                    sb.AppendLine(sectionBase.OffsetX.ToString("G"));
                    sb.Append("OffsetZ: ");
                    sb.AppendLine(sectionBase.OffsetZ.ToString("G"));
                    sb.Append("Priority: ");
                    sb.AppendLine(sectionBase.Priority.ToString("G"));
                    sb.Append("RowNumber: ");
                    sb.AppendLine(sectionBase.RowNumber.ToString("G"));

                    foreach (KeyValuePair<UserParameter, double> userParameter in sectionBase.UserParameters)
                    {
                        sb.Append($"{userParameter.Key}: ");
                        sb.AppendLine(userParameter.Value.ToString("G"));
                    }

                    sb.Append("ReferableSide: ");
                    sb.AppendLine(sectionBase.ReferableSide.ToString());

                    if (sectionBase.ReferredSection != null)
                    {
                        sb.Append("ReferredSection.MaterialAlias: ");
                        sb.AppendLine(sectionBase.ReferredSection.MaterialAlias);
                        sb.Append("ReferredSection.StartX: ");
                        sb.AppendLine(sectionBase.ReferredSection.StartX.ToString("G"));
                        sb.Append("ReferredSection.EndX: ");
                        sb.AppendLine(sectionBase.ReferredSection.EndX.ToString("G"));
                        sb.Append("ReferredSection.StartZ: ");
                        sb.AppendLine(sectionBase.ReferredSection.StartZ.ToString("G"));
                        sb.Append("ReferredSection.EndZ: ");
                        sb.AppendLine(sectionBase.ReferredSection.EndZ.ToString("G"));
                        sb.Append("ReferredSection.WidthZ: ");
                        sb.AppendLine(sectionBase.ReferredSection.WidthZ(0).ToString("G"));
                    }
                }
            }

            Save(sb, filePath, appendLine);
        }

        public static void Print(Complex complex, string extraMarker)
        {
            string ending = string.IsNullOrEmpty(extraMarker) ? string.Empty : $"_{extraMarker}";
            string filePath = Path.Combine(DirectoryPath, $"Complex{ending}.txt");

            var sb = new StringBuilder();
            sb.Append("Real: ");
            sb.AppendLine(complex.Real.ToString("G"));
            sb.Append("Imag: ");
            sb.AppendLine(complex.Imaginary.ToString("G"));

            Save(sb, filePath);
        }

        private static void Save(StringBuilder sb, string filePath, bool appendLine = false)
        {
            var directoryName =  Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(directoryName))
            {
                // TODO log something
                return;
            }

            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            using (var stream = File.AppendText(filePath))
            {
                if (appendLine)
                {
                    stream.WriteLine();
                }

                stream.WriteLine(sb);
            }
        }

        private static int GetNextCount(string key, bool appendLine)
        {
            if (Counter2FilePath.TryGetValue(key, out var counter))
            {
                if (!appendLine)
                {
                    counter = 0;
                }
                else
                {
                    counter++;
                }

                Counter2FilePath[key] = counter;
            }
            else
            {
                Counter2FilePath.Add(key, counter);
            }

            return counter;
        }
    }
}
