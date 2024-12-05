using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
    public void Rotate() {
        transform.Rotate(new Vector3(0, 0, 90));
    }

    public void SetDimensions(float length, float width) {
        transform.localScale = new Vector3(100 * length, width);
    }

    public bool IsWithinWall(Vector3 position, Cell parentCell) {
        bool isWithinWall = false;

        // Debug.Log("Position to check, X: " + position.x + ", Y: " + position.y);
        
        if (IsRotated()) {
            float maxX = transform.position.x + parentCell.GetWallThickness();
            float minX = transform.position.x - parentCell.GetWallThickness();
            float maxY = parentCell.transform.position.y + (parentCell.GetLength() / 2);
            float minY = parentCell.transform.position.y - (parentCell.GetLength() / 2);

            // Debug.Log("Max Y: " + maxY + ", Min Y: " + minY + ", Max X: " + maxX + ", Min X: " + minX + ", Wall Thickness: " + parentCell.GetWallThickness());

            if (minX <= position.x && position.x <= maxX && minY <= position.y && position.y <= maxY) {
                isWithinWall = true;
            }
        } else {
            float maxX = parentCell.transform.position.x + (parentCell.GetWidth() / 2);
            float minX = parentCell.transform.position.x - (parentCell.GetWidth() / 2);
            float maxY = transform.position.y + parentCell.GetWallThickness();
            float minY = transform.position.y - parentCell.GetWallThickness();

            // Debug.Log("Max Y: " + maxY + ", Min Y: " + minY + ", Max X: " + maxX + ", Min X: " + minX + ", Wall Thickness: " + parentCell.GetWallThickness());

            if (minX <= position.x && position.x <= maxX && minY <= position.y && position.y <= maxY) {
                isWithinWall = true;
            }
        }
        return isWithinWall;
    }

    public List<Wall> SplitWall(Vector3 position, float widthOfGap, Cell parentCell) {
        Debug.Log("Target Position: (" + position.x + "," + position.y + ")");
        Debug.Log("Gap Width: " + widthOfGap);

        // widthOfGap / 2 + (remainder length of wall / 2)
        List<Wall> splitWalls = new List<Wall>();
        Vector3 rightWallPosition;
        Vector3 leftWallPosition;
        float rightRemainder;
        float leftRemainder;
        float rightScalePercentage;
        float leftScalePercentage;
        float rightTotalPercentage;
        float leftTotalPercentage;
        
        
        if (IsRotated()) {
            float maxY = parentCell.transform.position.y + (parentCell.GetLength() / 2);
            float minY = parentCell.transform.position.y - (parentCell.GetLength() / 2);
            rightRemainder = maxY - position.y - (widthOfGap / 2);
            leftRemainder = position.y - minY - (widthOfGap / 2);
            rightScalePercentage = rightRemainder / (maxY - position.y);
            leftScalePercentage = leftRemainder / (position.y - minY);
            rightTotalPercentage = (maxY - position.y) / (maxY - minY);
            leftTotalPercentage = (position.y - minY) / (maxY - minY);

            Debug.Log("Max Y: " + maxY + ", Min Y: " + minY + ", Right Remainder: " + rightRemainder + ", Left Remainder: " + leftRemainder);
            Debug.Log("Right Scale %: " + rightScalePercentage + ", Left Scale %: " + leftScalePercentage);
            Debug.Log("Right Remainder %: " + rightTotalPercentage + ", Left Remainder %: " + leftTotalPercentage);

            rightWallPosition = new Vector3(transform.position.x, maxY - (rightRemainder / 2));
            leftWallPosition = new Vector3(transform.position.x, minY + (leftRemainder / 2));
        } else {
            float maxX = parentCell.transform.position.x + (parentCell.GetWidth() / 2);
            float minX = parentCell.transform.position.x - (parentCell.GetWidth() / 2);
            rightRemainder = maxX - position.x - (widthOfGap / 2);
            leftRemainder = position.x - minX - (widthOfGap / 2);
            rightScalePercentage = rightRemainder / (maxX - position.x);
            leftScalePercentage = leftRemainder / (position.x - minX);
            rightTotalPercentage = (maxX - position.x) / (maxX - minX);
            leftTotalPercentage = (position.x - minX) / (maxX - minX);

            Debug.Log("Max X: " + maxX + ", Min X: " + minX + ", Right Remainder: " + rightRemainder + ", Left Remainder: " + leftRemainder);
            Debug.Log("Right Scale %: " + rightScalePercentage + ", Left Scale %: " + leftScalePercentage);
            Debug.Log("Right Remainder %: " + rightTotalPercentage + ", Left Remainder %: " + leftTotalPercentage);
            
            rightWallPosition = new Vector3(maxX - (rightRemainder / 2), transform.position.y);
            leftWallPosition = new Vector3(minX + (leftRemainder / 2), transform.position.y);
        }

        Wall rightWall = Instantiate(this, rightWallPosition, Quaternion.identity);
        Wall leftWall = Instantiate(this, leftWallPosition, Quaternion.identity);

        float rightWallScale = rightWall.GetWidth() * rightTotalPercentage * rightScalePercentage;
        float leftWallScale = leftWall.GetWidth() * leftTotalPercentage * leftScalePercentage;

        Debug.Log("Right Scale Value: " + rightWallScale + ", Left Scale Value: " + leftWallScale);

        rightWall.transform.localScale = new Vector3(rightWallScale, rightWall.GetLength());
        leftWall.transform.localScale = new Vector3(leftWallScale, leftWall.GetLength());

        if (IsRotated()) {
            rightWall.Rotate();
            leftWall.Rotate();
        }

        if (rightRemainder <= 0) {
            // No wall left
            Destroy(rightWall.gameObject);
        } else {
            splitWalls.Add(rightWall);
        }
        if (leftRemainder <= 0) {
            // No wall left
            Destroy(leftWall.gameObject);
        } else {
            splitWalls.Add(leftWall);
        }

        Destroy(this.gameObject);

        return splitWalls;
    }

    private bool IsRotated() {
        return transform.rotation.eulerAngles.z == 90;
    }

    public float GetWidth() {
        return transform.localScale.x;
    }

    public float GetLength() {
        return transform.localScale.y;
    }
}
