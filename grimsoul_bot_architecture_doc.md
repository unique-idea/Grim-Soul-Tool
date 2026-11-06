# TÀI LIỆU THIẾT KẾ KỸ THUẬT - AUTO FARM BOT FOR GRIM SOUL (TOP-DOWN SURVIVAL)

## 1. TỔNG QUAN DỰ ÁN (PROJECT OVERVIEW)
* **Mục tiêu:** Xây dựng Tool Auto Farm tài nguyên và tự động chiến đấu cơ bản trên trình giả lập Android (LDPlayer/BlueStacks) cho game *Grim Soul: Dark Survival*.
* **Ngôn ngữ & Framework:** C# (.NET 8.0)
* **Kiến trúc giải pháp:**
  * `GrimSoul.BotEngine` (Class Library): Chứa Core logic, Vision processing, Win32 API, Grid System.
  * `GrimSoul.BotUI` (WPF Application): Chứa Overlay chọn vùng màn hình (Custom Area Selection), Toolbar điều chỉnh độ mịn Grid và giao diện điều khiển.

---

## 2. CẤU TRÚC KIẾN TRÚC DỰ ÁN (PROJECT STRUCTURE)

```text
GrimSoulBot.sln
│
├── GrimSoul.BotEngine/               [Class Library Project]
│   ├── Win32/
│   │   ├── NativeMethods.cs          (DllImport User32.dll: SendInput, SetCursorPos, mouse_event)
│   │   └── WindowHelper.cs           (Xác định Handle HWND & Capture màn hình)
│   │
│   ├── Vision/
│   │   ├── ImageProcessor.cs         (MatchTemplate, Color Range HSV Filtering)
│   │   └── OcrService.cs             (Đọc các chỉ số con số/chữ nếu cần)
│   │
│   ├── Grid/
│   │   ├── GridCell.cs               (Struct đại diện tọa độ A1-Zx)
│   │   ├── GridMatrix.cs             (Chuyển đổi Pixel <-> Ô lưới Grid)
│   │   └── PathFinder.cs             (Thuật toán Spiral Search & Quy đổi Hướng di chuyển W/A/S/D)
│   │
│   └── Core/
│       ├── BotStateMachine.cs        (Quản lý trạng thái: Idle, Harvesting, Combat, Restock)
│       └── InventoryHandler.cs       (Xử lý tương tác Rương/Balo: Single/Double Click)
│
└── GrimSoul.BotUI/                   [WPF Project]
    ├── Windows/
    │   ├── OverlayWindow.xaml        (Khung nét đứt xuyên thấu ABCD kéo thả)
    │   └── MainWindow.xaml           (Bảng điều khiển chính)
    └── App.xaml
```

---

## 3. CƠ CHẾ VẬN HÀNH CỐT LÕI (CORE MECHANICS)

### 3.1. Hệ thống Lưới Tùy chỉnh (Dynamic Grid System)
* Người dùng kéo chọn vùng $ABCD$ đè lên giả lập.
* Khung chữ nhật có kích thước $Width \times Height$, góc trái trên $(X_{start}, Y_{start})$.
* Chia vùng thành ma trận $N \times N$ dựa theo giá trị Slider trên Toolbar.
* **Tâm lưới (Nhân vật):** Ô $P = (\lceil N/2 \rceil, \lceil N/2 \rceil)$.

#### Công thức Quy đổi Pixel -> Grid Cell:
$$\text{CellWidth} = \frac{Width}{N}, \quad \text{CellHeight} = \frac{Height}{N}$$
$$\text{Column (B)} = \left\lfloor \frac{X_{pixel} - X_{start}}{\text{CellWidth}} \right\rfloor + 1$$
$$\text{Row (A)} = \left\lfloor \frac{Y_{pixel} - Y_{start}}{\text{CellHeight}} \right\rfloor + 1$$

---

### 3.2. Thuật toán Quét Xoắn Ốc (Spiral Radius Search)
Quét từ bán kính $R = 1$ xung quanh ô Nhân vật $P(Row_P, Col_P)$ tăng dần lên $R_{max}$:

```csharp
public List<GridCell> GetCellsInRadius(GridCell playerCell, int radius, int gridSize)
{
    List<GridCell> cells = new List<GridCell>();
    for (int r = playerCell.Row - radius; r <= playerCell.Row + radius; r++)
    {
        for (int c = playerCell.Col - radius; c <= playerCell.Col + radius; c++)
        {
            if (r < 1 || r > gridSize || c < 1 || c > gridSize) continue;
            if (r == playerCell.Row && c == playerCell.Col) continue;

            int dist = Math.Max(Math.Abs(r - playerCell.Row), Math.Abs(c - playerCell.Col));
            if (dist == radius) cells.Add(new GridCell(r, c));
        }
    }
    return cells;
}
```

---

### 3.3. Mapping Hướng di chuyển & Phím Giả lập (8-Way Direction Movement)
Độ lệch: $\Delta Row = Row_{item} - Row_P$, $\Delta Col = Col_{item} - Col_P$.

| $\Delta Row$ | $\Delta Col$ | Hướng | Key Input |
| :--- | :--- | :--- | :--- |
| $< 0$ | $= 0$ | Bắc (North) | `W` |
| $> 0$ | $= 0$ | Nam (South) | `S` |
| $= 0$ | $> 0$ | Đông (East) | `D` |
| $= 0$ | $< 0$ | Tây (West) | `A` |
| $< 0$ | $> 0$ | Đông Bắc (NE) | Hold `W` + `D` |
| $< 0$ | $< 0$ | Tây Bắc (NW) | Hold `W` + `A` |
| $> 0$ | $> 0$ | Đông Nam (SE) | Hold `S` + `D` |
| $> 0$ | $< 0$ | Tây Nam (SW) | Hold `S` + `A` |

---

### 3.4. Tương tác Rương & Vật phẩm (Click / Double Click)
Sử dụng Win32 Interop cho các thao tác chuột chính xác cao:

```csharp
public static void DoubleClickAt(int screenX, int screenY, int intervalMs = 80)
{
    SetCursorPos(screenX, screenY);
    Thread.Sleep(30);
    
    // Click 1
    mouse_event(MOUSEEVENTF_LEFTDOWN, (uint)screenX, (uint)screenY, 0, 0);
    mouse_event(MOUSEEVENTF_LEFTUP, (uint)screenX, (uint)screenY, 0, 0);
    
    Thread.Sleep(intervalMs);
    
    // Click 2
    mouse_event(MOUSEEVENTF_LEFTDOWN, (uint)screenX, (uint)screenY, 0, 0);
    mouse_event(MOUSEEVENTF_LEFTUP, (uint)screenX, (uint)screenY, 0, 0);
}
```

---

## 4. QUY TRÌNH THỰC THI CHÍNH (STATE MACHINE FLOW)

1. **STATE 1: IDLE / CHECK_STATUS**
   * Chụp ảnh khung $ABCD$.
   * Kiểm tra độ bền công cụ (Pixel/MatchTemplate) & Thanh Máu (HP Bar).
   * Nếu Máu thấp $\rightarrow$ Bấm phím hồi máu. Nếu Tool gãy $\rightarrow$ Bấm crafting.

2. **STATE 2: SCAN_OBJECTS**
   * Quét các ô xung quanh theo bán kính mở rộng $R = 1, 2, 3...$
   * Dùng OpenCV nhận diện Icon / Màu sắc của vật phẩm (Cây, Đá, Quái).
   * Lọc qua Whitelist do User cấu hình.

3. **STATE 3: NAVIGATE_AND_HARVEST**
   * Lấy vật phẩm gần nhất thỏa mãn.
   * Tính toán hướng di chuyển ($\Delta Row, \Delta Col$).
   * Giữ phím `W/A/S/D` trong thời gian $T = R \times T_{cell}$.
   * Kích hoạt nút tương tác (Thu hoạch / Tấn công).

4. **STATE 4: CHEST_INTERACTION (Nếu mở rương/chuyển đồ)**
   * Dừng gửi lệnh di chuyển `W/A/S/D`.
   * Nhận diện lưới ô chứa đồ trong rương.
   * Thực hiện `DoubleClickAt` để chuyển vật phẩm về Balo nhân vật.
```
eof

```

I have created the architecture specification file `GrimSoul_Bot_Architecture.md`. 

### Tổng kết
* **Kiến trúc đề xuất:** Tách thành 2 Projects (`Class Library` cho Core Engine + `WPF` cho UI Overlay).
* **Mã nguồn/Tài liệu:** Đã tạo file `GrimSoul_Bot_Architecture.md` chứa toàn bộ thuật toán, cấu trúc file, công thức toán học và mã C# mẫu. Bạn chỉ cần chuyển tài liệu này cho AI Agent để bắt đầu viết code từng module!