using System;
using System.Collections.Generic;
using System.Linq;

public struct DataPoint
{
    public double ThicknessMm { get; }
    public double ResistanceHr { get; }
    public double ThicknessInches { get; }
    

    public DataPoint(double thicknessMm, double resistanceHr, double thicknessInches)
    {
        ThicknessMm = thicknessMm;
        ResistanceHr = resistanceHr;
        ThicknessInches = thicknessInches;
    }
}

public class FireResistanceCalculator
{
    // Use a Dictionary to store data for ALL aggregate types
    private Dictionary<AciAggregateCategory, List<DataPoint>> allAggregateData;

    public FireResistanceCalculator()
    {
        allAggregateData = new Dictionary<AciAggregateCategory, List<DataPoint>>();

        // ThicknessMm ascending!

        // new DataPoint(Millimeters, Fire resistance rating in hours)
        //LIGHTWEIGHT
        allAggregateData[AciAggregateCategory.Lightweight] = new List<DataPoint>
        {
            new DataPoint(50.0, 0.72, 2.0),
            new DataPoint(75.0, 1.43, 3.0),
            new DataPoint(100.0, 2.50, 4.0),
            new DataPoint(125.0, 3.85, 5.0),
            new DataPoint(145.0, 5.0, 5.67)
        };

        //SEMI-LIGHTWEIGHT
        allAggregateData[AciAggregateCategory.SemiLightweight] = new List<DataPoint>
        {
            new DataPoint(50.0, 0.7, 2.0),
            new DataPoint(75.0, 1.29, 3.0),
            new DataPoint(100.0, 2.25, 4.0),
            new DataPoint(125.0, 3.45, 5.0),
            new DataPoint(150.0, 5.0, 6.0)
        };

        //CARBONATE
        allAggregateData[AciAggregateCategory.Carbonate] = new List<DataPoint>
        {
            new DataPoint(50.0, 0.5, 2.0),
            new DataPoint(75.0, 0.91, 3.0),
            new DataPoint(100.0, 1.49, 4.0),
            new DataPoint(125.0, 2.23, 5.0),
            new DataPoint(150.0, 3.23, 6.0),
            new DataPoint(175.0, 4.28, 7.0)
        };

        //SILICEOUS
        allAggregateData[AciAggregateCategory.Siliceous] = new List<DataPoint>
        {
            new DataPoint(50.0, 0.45, 2.0),
            new DataPoint(75.0, 0.78, 3.0),
            new DataPoint(100.0, 1.32, 4.0),
            new DataPoint(125.0, 2.00, 5.0),
            new DataPoint(150.0, 2.79, 6.0),
            new DataPoint(175.0, 3.82, 7.0)
        };

        //INSULATING CONCRETE
        allAggregateData[AciAggregateCategory.Insulating] = new List<DataPoint>
        {
            new DataPoint(50.0, 1.24, 2.0),
            new DataPoint(75.0, 2.77, 3.0),
            new DataPoint(100.0, 5.0, 4.0)
        };

        //AIR-COOLED BLAST FURNACE SLAG
        allAggregateData[AciAggregateCategory.AirCooledSlag] = new List<DataPoint>
        {
            new DataPoint(50.0, 0.63, 2.0),
            new DataPoint(75.0, 1.13, 3.0),
            new DataPoint(100.0, 1.94, 4.0),
            new DataPoint(125.0, 2.8, 5.0),
            new DataPoint(150.0, 3.85, 6.0),
            new DataPoint(170.0, 5.00, 6.77)
        };
    }



public double LinearInterpolateCalc(double targetThicknessMm, AciAggregateCategory aggregateType)
    {
        // 1. Check if data exists for the requested aggregate type
        if (!allAggregateData.TryGetValue(aggregateType, out List<DataPoint> dataPoints))
        {
            // Handle error: Data not found for this aggregate type
            Console.WriteLine($"Error: No data found for aggregate type: {aggregateType}");
            return double.NaN; // Indicate error
        }

        // Check if the list is null or empty (shouldn't happen with current constructor)
        if (dataPoints == null || dataPoints.Count == 0)
        {
             Console.WriteLine($"Error: Data list is empty for aggregate type: {aggregateType}");
             return double.NaN;
        }


        // 2. Handle edge cases: Thickness outside the recorded range

        // If thickness is less than or equal to the first point's thickness
        if (targetThicknessMm <= dataPoints[0].ThicknessMm)
        {
            return dataPoints[0].ResistanceHr; // Return resistance of the first point
        }

        // If thickness is greater than or equal to the last point's thickness
        if (targetThicknessMm >= dataPoints[dataPoints.Count - 1].ThicknessMm)
        {
            return dataPoints[dataPoints.Count - 1].ResistanceHr; // Return resistance of the last point
        }

        // 3. Find the two points that bracket the target thickness
        DataPoint lowerPoint = dataPoints[0];
        DataPoint upperPoint = dataPoints[dataPoints.Count - 1];

        for (int i = 1; i < dataPoints.Count; i++)
        {
            // Use ThicknessMm for comparison
            if (dataPoints[i].ThicknessMm >= targetThicknessMm)
            {
                // We found the upper bound. The previous point is the lower bound.
                upperPoint = dataPoints[i];
                lowerPoint = dataPoints[i - 1];
                break; // Exit the loop once bounds are found
            }
        }

        // 4. Perform Linear Interpolation
        // Formula: y = y0 + (x - x0) * (y1 - y0) / (x1 - x0)
        double x = targetThicknessMm;
        double x0 = lowerPoint.ThicknessMm;
        double y0 = lowerPoint.ResistanceHr;
        double x1 = upperPoint.ThicknessMm;
        double y1 = upperPoint.ResistanceHr;

        // Avoid division by zero if points somehow have same thickness
        if (x1 - x0 == 0)
        {
            // This case shouldn't happen with distinct data points, but good to handle.
            // Return the resistance of either point, or average them.
            return y0;
        }

        double interpolatedResistance = y0 + (x - x0) * (y1 - y0) / (x1 - x0);

        return interpolatedResistance;
    }
}