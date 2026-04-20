using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 3×3 전투 그리드를 관리합니다.
/// 유닛 배치, 이동, 밀치기/당기기 등 위치 관련 모든 연산을 담당합니다.
/// </summary>
public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Grid Settings")]
    [SerializeField] private int columns = 3;
    [SerializeField] private int rows = 3;
    [SerializeField] private float cellSize = 1.5f;

    // 그리드 데이터: key = (col, row), value = 해당 칸의 유닛
    private Dictionary<Vector2Int, Unit> grid = new Dictionary<Vector2Int, Unit>();

    // 그리드 셀 오브젝트 (시각화용, 선택적)
    private GameObject[,] cellObjects;

    // ───────────────────────────────────────────
    // Unity 생명주기
    // ───────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        InitializeGrid();
    }

    // ───────────────────────────────────────────
    // 초기화
    // ───────────────────────────────────────────

    /// <summary>그리드를 초기화합니다. 모든 칸을 비웁니다.</summary>
    public void InitializeGrid()
    {
        grid.Clear();
        for (int c = 0; c < columns; c++)
            for (int r = 0; r < rows; r++)
                grid[new Vector2Int(c, r)] = null;

        Debug.Log($"[GridManager] {columns}×{rows} 그리드 초기화 완료");
    }

    // ───────────────────────────────────────────
    // 유닛 배치 / 제거
    // ───────────────────────────────────────────

    /// <summary>유닛을 지정한 칸에 배치합니다. 칸이 비어있을 때만 성공합니다.</summary>
    public bool PlaceUnit(Unit unit, Vector2Int cell)
    {
        if (!IsValidCell(cell))
        {
            Debug.LogWarning($"[GridManager] 유효하지 않은 칸: {cell}");
            return false;
        }
        if (!IsEmpty(cell))
        {
            Debug.LogWarning($"[GridManager] {cell} 칸이 이미 점유됨");
            return false;
        }

        grid[cell] = unit;
        unit.GridPosition = cell;
        unit.transform.position = CellToWorld(cell);
        return true;
    }

    /// <summary>유닛을 그리드에서 제거합니다.</summary>
    public void RemoveUnit(Unit unit)
    {
        if (grid.ContainsKey(unit.GridPosition))
            grid[unit.GridPosition] = null;
    }

    // ───────────────────────────────────────────
    // 이동
    // ───────────────────────────────────────────

    /// <summary>유닛을 목표 칸으로 이동합니다. 이동 불가 시 false를 반환합니다.</summary>
    public bool MoveUnit(Unit unit, Vector2Int targetCell)
    {
        if (!IsValidCell(targetCell) || !IsEmpty(targetCell))
            return false;

        grid[unit.GridPosition] = null;
        grid[targetCell] = unit;
        unit.GridPosition = targetCell;
        unit.transform.position = CellToWorld(targetCell);
        return true;
    }

    // ───────────────────────────────────────────
    // 밀치기 / 당기기
    // ───────────────────────────────────────────

    /// <summary>
    /// 유닛을 지정 방향으로 distance칸 밀어냅니다.
    /// 벽이나 다른 유닛에 막히면 이동 가능한 만큼만 이동합니다.
    /// </summary>
    public void PushUnit(Unit unit, Vector2Int direction, int distance)
    {
        for (int i = 0; i < distance; i++)
        {
            Vector2Int next = unit.GridPosition + direction;
            if (!IsValidCell(next) || !IsEmpty(next))
                break;
            MoveUnit(unit, next);
        }
    }

    /// <summary>
    /// 유닛을 지정 방향으로 distance칸 당겨옵니다. (밀치기의 반대 방향)
    /// </summary>
    public void PullUnit(Unit unit, Vector2Int direction, int distance)
    {
        PushUnit(unit, -direction, distance);
    }

    // ───────────────────────────────────────────
    // 조회 유틸리티
    // ───────────────────────────────────────────

    /// <summary>해당 칸의 유닛을 반환합니다. 없으면 null.</summary>
    public Unit GetUnitAt(Vector2Int cell)
    {
        if (!IsValidCell(cell)) return null;
        return grid[cell];
    }

    /// <summary>칸이 비어있는지 확인합니다.</summary>
    public bool IsEmpty(Vector2Int cell) => IsValidCell(cell) && grid[cell] == null;

    /// <summary>칸이 그리드 범위 내에 있는지 확인합니다.</summary>
    public bool IsValidCell(Vector2Int cell) =>
        cell.x >= 0 && cell.x < columns && cell.y >= 0 && cell.y < rows;

    /// <summary>그리드 내 모든 유닛 목록을 반환합니다.</summary>
    public List<Unit> GetAllUnits()
    {
        var units = new List<Unit>();
        foreach (var unit in grid.Values)
            if (unit != null) units.Add(unit);
        return units;
    }

    /// <summary>특정 팀의 모든 유닛을 반환합니다.</summary>
    public List<Unit> GetUnitsByTeam(TeamType team)
    {
        var units = new List<Unit>();
        foreach (var unit in grid.Values)
            if (unit != null && unit.Team == team) units.Add(unit);
        return units;
    }

    /// <summary>두 칸 사이의 맨해튼 거리를 반환합니다.</summary>
    public int GetManhattanDistance(Vector2Int a, Vector2Int b) =>
        Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

    // ───────────────────────────────────────────
    // 좌표 변환
    // ───────────────────────────────────────────

    /// <summary>그리드 좌표 → 월드 좌표 변환</summary>
    public Vector3 CellToWorld(Vector2Int cell)
    {
        float x = (cell.x - (columns - 1) / 2f) * cellSize;
        float y = (cell.y - (rows - 1) / 2f) * cellSize;
        return new Vector3(x, y, 0f);
    }

    /// <summary>월드 좌표 → 그리드 좌표 변환 (가장 가까운 칸)</summary>
    public Vector2Int WorldToCell(Vector3 worldPos)
    {
        int c = Mathf.RoundToInt(worldPos.x / cellSize + (columns - 1) / 2f);
        int r = Mathf.RoundToInt(worldPos.y / cellSize + (rows - 1) / 2f);
        return new Vector2Int(c, r);
    }

    // ───────────────────────────────────────────
    // 디버그
    // ───────────────────────────────────────────

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        for (int c = 0; c < columns; c++)
        {
            for (int r = 0; r < rows; r++)
            {
                Vector3 center = CellToWorld(new Vector2Int(c, r));
                Gizmos.DrawWireCube(center, Vector3.one * cellSize * 0.95f);
            }
        }
    }
}
