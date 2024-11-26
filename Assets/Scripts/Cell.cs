using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class Cell : MonoBehaviour {

    [Header("Aesthetics")]
    [SerializeField] GameObject wallPrefab;
    [SerializeField] float wallThickness = 0.025f;
    [SerializeField] float doorPositionX = 1f;
    [SerializeField] float doorPositionY = 1f;
    [SerializeField] float doorThickness = 0.5f;
    [SerializeField] float minRoomWidth = 0.0f;
    [SerializeField] float maxRoomWidth = 0.0f;
    [SerializeField] float minRoomHeight = 0.0f;
    [SerializeField] float maxRoomHeight = 0.0f;

    [SerializeField] bool isRoomCandidate = false;

    public void SetDimensions(float width, float height) {
        transform.localScale += new Vector3(width, height, 0);
        if (width > minRoomWidth && width < maxRoomWidth && height > minRoomHeight && height < maxRoomHeight) {
            isRoomCandidate = true;
        }
    }

    public void ShiftPosition(Vector3 shift) {
        transform.position += shift;
    }

    public bool IsOverlappingWith(Cell other) {
        var bottomLeft = transform.position - new Vector3(transform.localScale.x / 2, transform.localScale.y / 2);
        var topRight = transform.position + new Vector3(transform.localScale.x / 2, transform.localScale.y / 2);

        var otherBottomLeft = other.transform.position - new Vector3(other.transform.localScale.x / 2, other.transform.localScale.y / 2);
        var otherTopRight = other.transform.position + new Vector3(other.transform.localScale.x / 2, other.transform.localScale.y / 2);

        // if (RectA.Left < RectB.Right && RectA.Right > RectB.Left &&
        // RectA.Top > RectB.Bottom && RectA.Bottom < RectB.Top ) 
        bool isOverlapping = (bottomLeft.x < otherTopRight.x && topRight.x > otherBottomLeft.x &&
        topRight.y > otherBottomLeft.y && bottomLeft.y < otherTopRight.y);

        return isOverlapping;
    }

    void BuildWalls() {
        float wallOffset = wallThickness / 2 / 100;
        Debug.Log("Wall Offset: " + wallOffset);
        Vector3 topWallPosition = transform.position + new Vector3(0, (transform.localScale.y / 2) - wallOffset);
        Vector3 bottomWallPosition = transform.position + new Vector3(0, (-transform.localScale.y / 2) + wallOffset);
        Vector3 leftWallPosition = transform.position + new Vector3((-transform.localScale.x / 2) + wallOffset, 0);
        Vector3 rightWallPosition = transform.position + new Vector3((transform.localScale.x / 2) - wallOffset, 0);

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

        topWall.SetDimensions(transform.localScale.x, wallThickness);
        bottomWall.SetDimensions(transform.localScale.x, wallThickness);
        leftWall.SetDimensions(transform.localScale.y, wallThickness);
        rightWall.SetDimensions(transform.localScale.y, wallThickness);

        topWall.SplitWall(new Vector3(doorPositionX, doorPositionY), doorThickness, this);
    }

    public bool IsRoomCandidate() {
        return isRoomCandidate;
    }
}
