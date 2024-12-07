using System;
using System.Collections.Generic;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class Cell : MonoBehaviour {

    [Header("Core")]
    [SerializeField] bool isStatic = false;
    [SerializeField] bool isRoomCandidate = false;
    [SerializeField] bool canSpawnEnemies = true;
    [SerializeField] bool mustSpawnEnemies = false;

    [Header("Aesthetics")]
    [SerializeField] GameObject wallPrefab;
    [SerializeField] float wallThickness = 0.025f;
    // [SerializeField] float doorPositionX = 1f;
    // [SerializeField] float doorPositionY = 1f;
    [SerializeField] float minRoomWidth = 0.0f;
    [SerializeField] float maxRoomWidth = 0.0f;
    [SerializeField] float minRoomHeight = 0.0f;
    [SerializeField] float maxRoomHeight = 0.0f;

    private List<Wall> walls = new List<Wall>();
    

    void Awake() {
        // BuildWalls();
        // Top Doors
        // AddDoor(new Vector3(1f, 0.5f));
        // AddDoor(new Vector3(0.7f, 0.5f));
        // AddDoor(new Vector3(1.3f, 0.5f));
        // AddDoor(new Vector3(0f, 0.5f));
        // Left Doors
        // AddDoor(new Vector3(1f, 0.4f));
        // AddDoor(new Vector3(1f, -0.4f));
    }

    public void SetDimensions(float width, float height) {
        transform.localScale += new Vector3(width, height, 0);
        if (width > minRoomWidth && width < maxRoomWidth && height > minRoomHeight && height < maxRoomHeight) {
            isRoomCandidate = true;
        }
    }

    public void ShiftPosition(Vector3 shift) {
        if (isStatic) {
            return;
        }
        transform.position += shift;
    }

    public bool IsOverlappingWith(Cell other) {
        var bottomLeft = transform.position - new Vector3(GetWidth() / 2, GetLength() / 2);
        var topRight = transform.position + new Vector3(GetWidth() / 2, GetLength() / 2);

        var otherBottomLeft = other.transform.position - new Vector3(other.GetWidth() / 2, other.GetLength() / 2);
        var otherTopRight = other.transform.position + new Vector3(other.GetWidth() / 2, other.GetLength() / 2);

        // if (RectA.Left < RectB.Right && RectA.Right > RectB.Left &&
        // RectA.Top > RectB.Bottom && RectA.Bottom < RectB.Top ) 
        bool isOverlapping = (bottomLeft.x < otherTopRight.x && topRight.x > otherBottomLeft.x &&
        topRight.y > otherBottomLeft.y && bottomLeft.y < otherTopRight.y);

        return isOverlapping;
    }

    public bool IsOverlappingXAxisWith(Cell other) {
        var minX = transform.position.x - (GetWidth() / 2);
        var maxX = transform.position.x + (GetWidth() / 2);

        var otherMinX = other.transform.position.x - (other.GetWidth() / 2);
        var otherMaxX = other.transform.position.x + (other.GetWidth() / 2);

        float minimum = Math.Max(minX, otherMinX);
        float maximum = Math.Min(maxX, otherMaxX);

        bool isOverlapping = (minimum < maximum);

        return isOverlapping;
    }

    public bool IsOverlappingYAxisWith(Cell other) {
        var minY = transform.position.y - (GetLength() / 2);
        var maxY = transform.position.y + (GetLength() / 2);

        var otherMinY = other.transform.position.y - (other.GetLength() / 2);
        var otherMaxY = other.transform.position.y + (other.GetLength() / 2);

        float minimum = Math.Max(minY, otherMinY);
        float maximum = Math.Min(maxY, otherMaxY);

        bool isOverlapping = (minimum < maximum);

        return isOverlapping;
    }

    public Vector3 GetBorderConnectionVector3(Cell other) {
        Vector3 borderVector;
        float x = 0;
        float y = 0;

        Vector3 topBorder = GetBorderVector3("top");
        Vector3 bottomBorder = GetBorderVector3("bottom");
        Vector3 leftBorder = GetBorderVector3("left");
        Vector3 rightBorder = GetBorderVector3("right");

        Vector3 otherTopBorder = other.GetBorderVector3("top");
        Vector3 otherBottomBorder = other.GetBorderVector3("bottom");
        Vector3 otherLeftBorder = other.GetBorderVector3("left");
        Vector3 otherRightBorder = other.GetBorderVector3("right");

        // Debug.Log("CURRN Top: " + topBorder + ", Bottom: " + bottomBorder + ", Left: " + leftBorder + ", Right: " + rightBorder);
        // Debug.Log("OTHER Top: " + otherTopBorder + ", Bottom: " + otherBottomBorder + ", Left: " + otherLeftBorder + ", Right: " + otherRightBorder);

        if (IsOverlappingXAxisWith(other)) {
            // Debug.Log("Is Overlapping on X-Axis");
            // Get Overlapping Min/Max X
            float minRangeX = Math.Max(leftBorder.x, otherLeftBorder.x);
            float maxRangeX = Math.Min(rightBorder.x, otherRightBorder.x);
            x = minRangeX + ((maxRangeX - minRangeX) / 2); // Mindpoint within overlapping range

            // Top or bottom border?
            if (topBorder.y < otherBottomBorder.y) {
                // Below Other Cell
                y = topBorder.y;
            } else if (bottomBorder.y > otherTopBorder.y) {
                // Above Other Cell
                y = bottomBorder.y;
            } else {
                Debug.Log("Unexpected Positions Between Cells!");
            }
        } else if (IsOverlappingYAxisWith(other)) {
            // Debug.Log("Is Overlapping on Y-Axis");
            // Get Overlapping Min/Max Y
            float minRangeY = Math.Max(bottomBorder.y, otherBottomBorder.y);
            float maxRangeY = Math.Min(topBorder.y, otherTopBorder.y);
            y = minRangeY + ((maxRangeY - minRangeY) / 2); // Mindpoint within overlapping range

            // Left or right border?
            if (rightBorder.x < otherLeftBorder.x) {
                // Left of Other Cell
                x = rightBorder.x;
            } else if (leftBorder.x > otherRightBorder.x) {
                // Right of Other Cell
                x = leftBorder.x;
            } else {
                Debug.Log("Unexpected Positions Between Cells!");
            }
        } else {
            // TODO: L-Shaped Connection
            Debug.Log("L-Shaped Connection Detected...");
            Vector3 cornerVector = Vector3.zero;
            Vector3 otherCornerVector = Vector3.zero;
            bool isTopBorderConnection = true;
            bool isBottomBorderConnection = true;
            bool isLeftBorderConnection = true;
            bool isRightBorderConnection = true;
            float halfPathOffset = (0.7f / 2) + 0.025f;

            if (topBorder.y <= otherBottomBorder.y) {
                isBottomBorderConnection = false;
            }
            if (bottomBorder.y >= otherTopBorder.y) {
                isTopBorderConnection = false;
            }
            if (leftBorder.x >= otherRightBorder.x) {
                isRightBorderConnection = false;
            }
            if (rightBorder.x <= otherLeftBorder.x) {
                isLeftBorderConnection = false;
            }

            if (isTopBorderConnection && isRightBorderConnection) {
                // Top Right Corner
                cornerVector = new Vector3(rightBorder.x, topBorder.y);
                otherCornerVector = new Vector3(otherLeftBorder.x, otherBottomBorder.y);
                Debug.Log("Top Right Corner: " + cornerVector);
                float cornerPositionFromLine = DistanceFromLine(transform.position, other.transform.position, cornerVector);
                float otherCornerPositionFromLine = DistanceFromLine(transform.position, other.transform.position, otherCornerVector);
                float direction = cornerPositionFromLine;
                
                if (Mathf.Abs(cornerPositionFromLine) < Mathf.Abs(otherCornerPositionFromLine)) {
                    // Compare magnitudes and go with the stronger one
                    direction = otherCornerPositionFromLine;
                }

                if (direction < 0) {
                    // Border on right
                    x = cornerVector.x;
                    y = cornerVector.y - halfPathOffset;
                } else if (direction > 0) {
                    // Border on top
                    x = cornerVector.x - halfPathOffset;
                    y = cornerVector.y;
                } else {
                    // Corner Case
                    // Both are exactly in the corner. Coordinate with "Top Right" it needs to go to the right
                    x = cornerVector.x;
                    y = cornerVector.y - halfPathOffset;
                }
                
            } else if (isTopBorderConnection && isLeftBorderConnection) {
                // Top Left Corner
                cornerVector = new Vector3(leftBorder.x, topBorder.y);
                otherCornerVector = new Vector3(otherRightBorder.x, otherBottomBorder.y);
                Debug.Log("Top Left Corner: " + cornerVector);
                float cornerPositionFromLine = DistanceFromLine(transform.position, other.transform.position, cornerVector);
                float otherCornerPositionFromLine = DistanceFromLine(transform.position, other.transform.position, otherCornerVector);
                float direction = cornerPositionFromLine;
                
                if (Mathf.Abs(cornerPositionFromLine) < Mathf.Abs(otherCornerPositionFromLine)) {
                    // Compare magnitudes and go with the stronger one
                    direction = otherCornerPositionFromLine;
                    Debug.Log("Switching to other corner's direction");
                }

                Debug.Log("Direction: " + direction + ", Corner Distance From Line: " + cornerPositionFromLine + ", Other Corner Distance From Line: " + otherCornerPositionFromLine);

                if (direction > 0) {
                    // Border on top
                    x = cornerVector.x + halfPathOffset;
                    y = cornerVector.y;
                } else if (direction < 0) {
                    // Border on left
                    x = cornerVector.x;
                    y = cornerVector.y - halfPathOffset;
                } else {
                    // Corner Case
                    // Both are exactly in the corner. Coordinate with "Bottom Right" it needs to go to the bottom
                    x = cornerVector.x;
                    y = cornerVector.y - halfPathOffset;
                }
            } else if (isBottomBorderConnection && isLeftBorderConnection) {
                // Bottom Left Corner
                cornerVector = new Vector3(leftBorder.x, bottomBorder.y);
                otherCornerVector = new Vector3(otherRightBorder.x, otherTopBorder.y);
                Debug.Log("Bottom Left Corner: " + cornerVector);
                float cornerPositionFromLine = DistanceFromLine(transform.position, other.transform.position, cornerVector);
                float otherCornerPositionFromLine = DistanceFromLine(transform.position, other.transform.position, otherCornerVector);
                float direction = cornerPositionFromLine;
                
                if (Mathf.Abs(cornerPositionFromLine) < Mathf.Abs(otherCornerPositionFromLine)) {
                    // Compare magnitudes and go with the stronger one
                    direction = otherCornerPositionFromLine;
                }
                if (direction > 0) {
                    // Border on bottom
                    x = cornerVector.x + halfPathOffset;
                    y = cornerVector.y;
                } else if (direction < 0) {
                    // Border on left
                    x = cornerVector.x;
                    y = cornerVector.y + halfPathOffset;
                } else {
                    // Corner Case
                    // Both are exactly in the corner. Coordinate with "Top Right" it needs to go to the right
                    x = cornerVector.x;
                    y = cornerVector.y + halfPathOffset;
                }
            } else if (isBottomBorderConnection && isRightBorderConnection) {
                // Bottom Right Corner
                cornerVector = new Vector3(rightBorder.x, bottomBorder.y);
                otherCornerVector = new Vector3(otherLeftBorder.x, otherTopBorder.y);
                Debug.Log("Bottom Right Corner: " + cornerVector);
		        float cornerPositionFromLine = DistanceFromLine(transform.position, other.transform.position, cornerVector);
                float otherCornerPositionFromLine = DistanceFromLine(transform.position, other.transform.position, otherCornerVector);
                float direction = cornerPositionFromLine;
                
                if (Mathf.Abs(cornerPositionFromLine) < Mathf.Abs(otherCornerPositionFromLine)) {
                    // Compare magnitudes and go with the stronger one
                    direction = otherCornerPositionFromLine;
                }
                if (direction < 0) {
                    // Border on right
                    x = cornerVector.x;
                    y = cornerVector.y + halfPathOffset;
                } else if (direction > 0) {
                    // Border on bottom
                    x = cornerVector.x - halfPathOffset;
                    y = cornerVector.y;
                } else {
                    // Corner Case
                    // Both are exactly in the corner. Coordinate with "Top Left" it needs to go to the bottom
                    x = cornerVector.x - halfPathOffset;
                    y = cornerVector.y;
                }

            } else {
                Debug.Log("Unknown State!");
            }
        }

        borderVector = new Vector3(x, y);
        Debug.Log("Border Vector: " + borderVector);

        return borderVector;
    }

    private bool IsLeftOfLine(Vector3 sourceOfLine, Vector3 destinationOfLine, Vector3 pointToCheck) {
        return (destinationOfLine.x - sourceOfLine.x) * (pointToCheck.y - sourceOfLine.y) > (destinationOfLine.y - sourceOfLine.y) * (pointToCheck.x - sourceOfLine.x);
    }

    private float DistanceFromLine(Vector3 sourceOfLine, Vector3 destinationOfLine, Vector3 pointToCheck) {
        return Mathf.Abs((destinationOfLine.y - sourceOfLine.y) * (pointToCheck.x - sourceOfLine.x)) - Mathf.Abs((destinationOfLine.x - sourceOfLine.x) * (pointToCheck.y - sourceOfLine.y));
    }

    public Vector3 GetBorderVector3(string direction) {
        Vector3 borderVector;
        float x = 0;
        float y = 0;

        if (direction.ToLower().Equals("top")) {
            x = transform.position.x;
            y = transform.position.y + (GetLength() / 2);
        } else if (direction.ToLower().Equals("bottom")) {
            x = transform.position.x;
            y = transform.position.y - (GetLength() / 2);
        } else if (direction.ToLower().Equals("left")) {
            x = transform.position.x - (GetWidth() / 2);
            y = transform.position.y;
        } else if (direction.ToLower().Equals("right")) {
            x = transform.position.x + (GetWidth() / 2);
            y = transform.position.y;
        }

        borderVector = new Vector3(x, y);

        return borderVector;
    }

    public void BuildWalls() {
        float wallOffset = GetWallThickness() / 2;
        // Debug.Log("Wall Offset: " + wallOffset);
        Vector3 topWallPosition = transform.position + new Vector3(0, (GetLength() / 2) - wallOffset);
        Vector3 bottomWallPosition = transform.position + new Vector3(0, (-GetLength() / 2) + wallOffset);
        Vector3 leftWallPosition = transform.position + new Vector3((-GetWidth() / 2) + wallOffset, 0);
        Vector3 rightWallPosition = transform.position + new Vector3((GetWidth() / 2) - wallOffset, 0);

        GameObject topWallObject = Instantiate(wallPrefab, topWallPosition, Quaternion.identity);
        GameObject bottomWallObject = Instantiate(wallPrefab, bottomWallPosition, Quaternion.identity);
        GameObject leftWallObject = Instantiate(wallPrefab, leftWallPosition, Quaternion.identity);
        GameObject rightWallObject = Instantiate(wallPrefab, rightWallPosition, Quaternion.identity);

        Wall topWall = topWallObject.GetComponent<Wall>();
        Wall bottomWall = bottomWallObject.GetComponent<Wall>();
        Wall leftWall = leftWallObject.GetComponent<Wall>();
        Wall rightWall = rightWallObject.GetComponent<Wall>();

        leftWall.Rotate();
        rightWall.Rotate();

        topWall.SetDimensions(GetWidth(), wallThickness);
        bottomWall.SetDimensions(GetWidth(), wallThickness);
        leftWall.SetDimensions(GetLength(), wallThickness);
        rightWall.SetDimensions(GetLength(), wallThickness);

        walls.Add(topWall);
        walls.Add(bottomWall);
        walls.Add(leftWall);
        walls.Add(rightWall);
    }

    public void AddDoor(Vector3 position, float doorThickness) {
        // Debug.Log("Adding Door at Vector X: " + position.x + ", Y: " + position.y);
        List<Wall> wallsToAdd = new List<Wall>();
        List<Wall> wallsToRemove = new List<Wall>();

        foreach (Wall wall in walls) {
            // Debug.Log("Checking if vector is within wall...");
            if (wall.IsWithinWall(position, this)) {
                wallsToRemove.Add(wall);
                // Debug.Log("Vector IS within wall! Position: " + position + ", Wall: " + wall.transform.position);
                wallsToAdd.AddRange(wall.SplitWall(position, doorThickness));
            }
        }

        foreach (Wall wall in wallsToRemove) {
            // Debug.Log("Removing Wall: " + wall.transform.position);
            walls.Remove(wall);
        }

        walls.AddRange(wallsToAdd);
    }

    public bool IsRoomCandidate() {
        return isRoomCandidate;
    }

    public float GetWidth() {
        return transform.localScale.x;
    }

    public float GetLength() {
        return transform.localScale.y;
    }

    public float GetWallThickness() {
        return wallThickness / 100;
    }

    public bool IsStatic() {
        return isStatic;
    }

    public bool CanSpawnEnemies() {
        return canSpawnEnemies;
    }

    public bool MustSpawnEnemies() {
        return mustSpawnEnemies;
    }
}
