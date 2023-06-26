using Game.Shared.DataModels;
using Game.UnitController;
using NUnit.Framework;
using System.Collections.Generic;

public class HexPathTests
{
    [Test]
    public void TestDirectionDown()
    {
        var startCoord = new Hex.Coord(5, 7);
        var controllerPathRef = new ControllerPath();

        var expected = new List<Hex.Coord>()
        {
            new Hex.Coord(-1, -1),
            new Hex.Coord(-1, -2),
            new Hex.Coord(-1, -3),
        };
        var givenPath = new List<Hex.Coord>()
        {
            new Hex.Coord(-1, -2),
            new Hex.Coord(-1, -1),
            new Hex.Coord(-1, 0),
        };

        var path = controllerPathRef.FormatPath(givenPath, startCoord);
        Assert.AreEqual(expected, path);

        controllerPathRef.ApplyPathToWorld(path);

        //controllerPathRef.ResetPathfindingVariables();

        expected = new List<Hex.Coord>()
        {
            new Hex.Coord(1, 0),
            new Hex.Coord(2, 1),
            new Hex.Coord(3, 1),
        };
        givenPath = new List<Hex.Coord>()
        {
            new Hex.Coord(3, 2),
            new Hex.Coord(2, 1),
            new Hex.Coord(1, 1),
        };

        path = controllerPathRef.FormatPath(givenPath, startCoord);
        Assert.AreEqual(expected, path);

        // Use the Assert class to test conditions
    }
}