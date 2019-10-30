using UnityEngine;

public class HexCell : MonoBehaviour {

    public HexCoordinates coordinates;
    public RectTransform uiRect;
    public Vector3 Position { get { return transform.localPosition; } }
    public HexGridChunk chunk;

    bool hasIncomingRiver, hasOutGoingRiver;
    HexDirection incomingRiver, outGoingRiver;

    public bool HasIncomingRiver{ get { return hasIncomingRiver; } }
    public bool HasOutGoingRiver { get { return hasOutGoingRiver; } }
    public HexDirection IncomingRiveer { get { return incomingRiver; } }
    public HexDirection OutcomingRienver { get { return outGoingRiver; } }

    public bool HasRiver { get { return hasIncomingRiver || hasOutGoingRiver; } }
    public bool HasRiverBeginOrEnd { get { return hasIncomingRiver != hasOutGoingRiver; } }

    public Color Color
    {
        get
        {
            return color;
        }
        set
        {
            if (color == value)
            {
                return;
            }
            color = value;
            Refresh();
        }
    }

    public int Elevation
    {
        get
        { return elevation; }
        set
        {
            if (elevation == value) return;
            elevation = value;

            Vector3 position = transform.localPosition;
            position.y = elevation * HexMetrics.elevationStep;
            position.y += (HexMetrics.SampleNoise(position).y * 2f - 1f) * HexMetrics.elevationPerturbStrength;
            transform.localPosition = position;

            Vector3 uiPosition = uiRect.localPosition;
            uiPosition.z = -position.y;
            uiRect.localPosition = uiPosition;

            if (hasOutGoingRiver && elevation < GetNeighbor(outGoingRiver).elevation)
            {
                RemoveOutGoingRiver();
            }
            if (HasIncomingRiver && elevation > GetNeighbor(outGoingRiver).elevation)
            {
                RemoveInComingRiver();
            }
            Refresh();
        }
    }

    private int elevation = int.MinValue;
    private Color color;

    [SerializeField]
	HexCell[] neighbors;

	public HexCell GetNeighbor (HexDirection direction) {
		return neighbors[(int)direction];
	}

	public void SetNeighbor (HexDirection direction, HexCell cell) {
		neighbors[(int)direction] = cell;
		cell.neighbors[(int)direction.Opposite()] = this;
	}

    public HexEdgeType GetEdgeType(HexDirection direction)
    {
        return HexMetrics.GetEdgeType(elevation, neighbors[(int)direction].elevation);
    }

    public HexEdgeType GetEdgeType(HexCell otherCell)
    {
        return HexMetrics.GetEdgeType(elevation, otherCell.elevation);
    }

    void Refresh()
    {
        if (chunk)
        {
            chunk.Refresh();

            for (int i = 0; i < neighbors.Length; i++)
            {
                HexCell neighbor = neighbors[i];
                if (neighbor != null && neighbor.chunk != chunk)
                {
                    neighbor.chunk.Refresh();
                }
            }
        }
    }

    void RefreshSelfOnly()
    {
        chunk.Refresh();
    }

    public bool HasRiverThroughEdge(HexDirection direction)
    {
        return hasIncomingRiver && incomingRiver == direction || hasOutGoingRiver && outGoingRiver == direction;
    }

    public void RemoveOutGoingRiver()
    {
        if (!hasOutGoingRiver) return;
        hasOutGoingRiver = false;
        RefreshSelfOnly();

        HexCell neighbor = GetNeighbor(outGoingRiver);
        neighbor.hasIncomingRiver = false;
        neighbor.RefreshSelfOnly();
    }

    public void RemoveInComingRiver()
    {
        if (!hasIncomingRiver) return;
        hasIncomingRiver = false;
        RefreshSelfOnly();

        HexCell neighbor = GetNeighbor(incomingRiver);
        neighbor.hasIncomingRiver = false;
        neighbor.RefreshSelfOnly();
    }

    public void RemoveRiver()
    {
        RemoveOutGoingRiver();
        RemoveInComingRiver();
    }

    public void SetOutGoingRiver(HexDirection direction)
    {
        if (hasOutGoingRiver && outGoingRiver == direction) { return; }

        HexCell neighbor = GetNeighbor(direction);
        if (!neighbor || elevation < neighbor.elevation) { return; }

        RemoveOutGoingRiver();
        if (hasIncomingRiver && incomingRiver == direction) { RemoveInComingRiver(); }

        hasOutGoingRiver = true;
        outGoingRiver = direction;
        RefreshSelfOnly();

        neighbor.RemoveInComingRiver();
        neighbor.hasIncomingRiver = true;
        neighbor.incomingRiver = direction.Opposite();
        neighbor.RefreshSelfOnly();
    }
}