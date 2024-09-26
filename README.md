![Screenshot 2024-09-11 144739](https://github.com/user-attachments/assets/5d8dd0c5-db07-49b0-a9e1-6cdeaa210884) 
# OmniCA 

Experimental Unity3D tool for simulating all possible 2D, two-state cellular automata on a square grid. This tool implements a 2D version of [Wolfram's Elementary Cellular Automaton (CA)](https://en.wikipedia.org/wiki/Elementary_cellular_automaton), where the neighborhood frame consists of 9 cells, resulting in a 512-bit rule size. This creates an immense rule space of 2^512, which equals approximately 1.34 × 10^154 possible rules.
The tool features a user-friendly interface for creating and saving rules, as well as for controlling initial conditions and simulation parameters. The simulation itself is highly efficient, running on the GPU and utilizing a compact representation of the rules. Users can create custom initial configurations, or utilize built-in options such as "Center", "Grid", "Noise", or "Paint". OmniCA also allows for chaining together rules to create more complex patterns and behavior.

## Usage Guide

Follow these steps to use it:

### 1. Open the Simulation Scene
- Locate the **SimulationManager** in the hierarchy.
  - You can find it under: `Simulation > Canvas > SimulationManager`.
  > ![Simulation Hierarchy](https://github.com/user-attachments/assets/26f523f6-c478-4f43-8809-78f8d6e3d960)

### 2. SimulationManager Parameters
The CAManager component allows you to control various aspects of the simulation:
> ![CAManager Inspector](https://github.com/user-attachments/assets/9b589b9b-6fdb-4d90-903b-868adabc8fdd)

#### Field Initializer
- Field for a script object that sets the initial field configuration.
> ![Select Field Initializer](https://github.com/user-attachments/assets/a941be3c-f4e2-4c1a-bc9e-0b343148d7d0) 
  - Standard options include **Center**, **Grid**, **Noise**, and **Paint**.
  - You can configure these or add new ones in the `Assets/Data/FieldInitializers` folder.
  - Create custom field initializers by creating classes derived from `FieldInitializer`. Refer to the existing ones for guidance.
  - **PaintFieldInitializer**: Let you paint the initial field in a special window or use `.png` files in the "Texture" field.
  > ![FieldEditor](https://github.com/user-attachments/assets/27520cb9-a735-4ace-a86d-41f16833f920)

#### Resolution
- Controls the field size. It must be a power of 2 for computational efficiency.

#### Rule Chain
- Allows you to experiment with rule changes within a session.
  >![Screenshot 2024-09-23 142100](https://github.com/user-attachments/assets/57ac3ff2-fb81-411f-bd12-651b4a8d072b)
  - **Queue Orders**: Choose from **Order**, **Repeat**, or **Random**.
  - The left side of the **Rules** element provides a dropdown list of saved rules, while the right side sets the number of steps before switching rules (0 for unlimited).
  - **Use Latest**: Applies the most recent unsaved rule.

### 3. Open Rule Editor
- Press the **Open Rule Editor** button to launch the Rule Editor window.

#### Rule Editor Overview
> ![Rule Editor Window](https://github.com/user-attachments/assets/060d263d-f266-414f-b660-41e8440ed65b)

- **File**: Save, load, or create new rule files.
  - It includes an input field and dropdown list of existing items, allowing you to edit rule names for saving.
  
- **Table**: The core of the rule editor, displaying all possible neighborhoods as grid-like icons.
  - A checkbox next to each icon indicates the next state of the cyan-highlighted cell.
  - Setting or clearing the checkbox changes the rule when the displayed configuration is encountered.

- **Search**: Quickly access any neighborhood configuration.
  - Click on cells in the grid to toggle their state between filled and empty.
  - Once selected, you can modify the rule by setting/clearing the central cell's next state.

- **Script**: Generate rules via scripts.
  - Select a script from the dropdown menu and press **Run** to execute it.
  - Create custom rule-generating scripts by extending the `RuleBuilder` class. Refer to existing scripts as examples.

- **Binary**: Displays the current rule as bits, reflecting the states from the **Table**.
  - For example, if config #5 is set to "true" in the **Table**, it will show as "1" in the corresponding index in the **Binary** section.

- **Integers**: Shows how the rule is stored in memory using 16 integers (512 bytes), representing the rule size.

### 4. Other CAManager Fields

- **Delay**: Controls the speed of the simulation. For example, `0.1` means a 0.1-second delay between frames (10 FPS).
- **Pause**: Pauses the simulation.
- **Step**: Allows for step-by-step simulation execution.
- **Restart**: Restarts the simulation from the beginning.

---

By adjusting these parameters within the **SimulationManager** and **Rule Editor**, you can explore and experiment with different cellular automata rules and configurations in OmniCA.



