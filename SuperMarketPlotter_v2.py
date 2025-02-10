import json
import matplotlib.pyplot as plt
import numpy as np
import matplotlib.patches as mpatches

def load_market_layout(file_path):
    """Loads the supermarket layout from a JSON file."""
    with open(file_path, 'r') as file:
        layout = json.load(file)
    return layout

def convert_layout_to_grid(layout):
    """Converts the layout JSON into a 2D grid."""
    rows, cols = layout['Rows'], layout['Cols']
    isle_matrix = layout['IsleMatrix']
    grid = np.array(isle_matrix).reshape(rows, cols)
    return grid

def find_positions(grid):
    """Finds positions of all aisles in the grid."""
    positions = {}
    for r in range(grid.shape[0]):
        for c in range(grid.shape[1]):
            if grid[r, c] not in ['0']:  # Exclude empty spaces
                positions[grid[r, c]] = (r, c)
    return positions

def generate_walking_path(positions, route, walking_pattern, cold_aisles):
    """Generates a step-by-step path following H2V, V2H, or ZgZg pattern with proper decay modeling."""
    path = route.split('->')
    path_coords = []
    step_numbers = []
    collected_items = []
    collected_cold_items = []
    foul_passes = []
    accumulated_time = 0
    accumulated_decay = 0
    current_cold_items = 0
    decay_over_time = []
    
    if '<' in positions:
        path_coords.append(positions['<'])
        step_numbers.append(0)
        decay_over_time.append((0, 0))
    
    step = 1
    for i in range(len(path) - 1):
        start = positions[path[i]]
        end = positions[path[i+1]]
        
        current_pos = start
        while current_pos != end:
            accumulated_time += 1
            
            # Update movement logic
            if walking_pattern == "H2V":
                if current_pos[1] != end[1]:  # Move horizontally first
                    current_pos = (current_pos[0], current_pos[1] + (1 if current_pos[1] < end[1] else -1))
                else:  # Then move vertically
                    current_pos = (current_pos[0] + (1 if current_pos[0] < end[0] else -1), current_pos[1])
            elif walking_pattern == "V2H":
                if current_pos[0] != end[0]:  # Move vertically first
                    current_pos = (current_pos[0] + (1 if current_pos[0] < end[0] else -1), current_pos[1])
                else:  # Then move horizontally
                    current_pos = (current_pos[0], current_pos[1] + (1 if current_pos[1] < end[1] else -1))
            else:  # Zigzag pattern
                if (current_pos[0] + current_pos[1]) % 2 == 0:
                    if current_pos[1] != end[1]:
                        current_pos = (current_pos[0], current_pos[1] + (1 if current_pos[1] < end[1] else -1))
                    else:
                        current_pos = (current_pos[0] + (1 if current_pos[0] < end[0] else -1), current_pos[1])
                else:
                    if current_pos[0] != end[0]:
                        current_pos = (current_pos[0] + (1 if current_pos[0] < end[0] else -1), current_pos[1])
                    else:
                        current_pos = (current_pos[0], current_pos[1] + (1 if current_pos[1] < end[1] else -1))
            
            # Update cold items count and decay
            if grid[current_pos] == path[i+1]:  # Ensure it's the next intended aisle in the route
                if grid[current_pos] in cold_aisles:
                    collected_cold_items.append((step, accumulated_time))
                    current_cold_items += 1  # Increase active decaying items
                else:
                    collected_items.append((step, accumulated_time))
            elif grid[current_pos] == 'x':  # Preserve foul aisle logic
                foul_passes.append((step, accumulated_time))
                accumulated_decay += 11 * current_cold_items  # Add foul aisle penalty
            else:
                accumulated_decay += current_cold_items  # Normal decay accumulation

                        
            decay_over_time.append((step, accumulated_decay))
            path_coords.append(current_pos)
            step_numbers.append(step)
            step += 1
    
    if '>' in positions:
        path_coords.append(positions['>'])
        step_numbers.append(step)
        decay_over_time.append((step, accumulated_decay))
    
    return path_coords, step_numbers, collected_items, collected_cold_items, foul_passes, decay_over_time

def plot_movement(time_steps, accumulated_time, accumulated_decay, collected_items, collected_cold_items, foul_passes):
    """Plots two movement plots: Accumulated Steps + Event Timeline, and Cold Food Decay."""
    fig, axs = plt.subplots(2, 1, figsize=(10, 9))
    
    # Ensure time_steps and accumulated_decay have the same length
    min_length = min(len(time_steps), len(accumulated_decay))
    time_steps = time_steps[:min_length]
    accumulated_decay = accumulated_decay[:min_length]
    
    # Combined Plot: Time vs Accumulated Steps + Event Timeline
    axs[0].plot(time_steps, accumulated_time, linestyle='-', linewidth=0.5, color='b', label="Accumulated Steps")
    if collected_items:
        axs[0].scatter(*zip(*collected_items), marker='s', color='g', label='Regular Item Collected')
    if collected_cold_items:
        axs[0].scatter(*zip(*collected_cold_items), marker='s', color='cyan', label='Cold Item Collected')
    if foul_passes:
        axs[0].scatter(*zip(*foul_passes), marker='x', color='r', label='Foul Passed')
    
    axs[0].set_xlabel("Time")
    axs[0].set_ylabel("Accumulated Steps")
    axs[0].set_title("Accumulated Time & Event Timeline")
    axs[0].legend()
    axs[0].grid(True)
    
    # Time vs Accumulated Cold Food Decay
    axs[1].plot(time_steps, accumulated_decay, marker='o', linestyle='-', color='r')
    axs[1].set_xlabel("Time")
    axs[1].set_ylabel("Accumulated Decay")
    axs[1].set_title("Cold Food Decay Over Time")
    axs[1].grid(True)
    
    plt.tight_layout()
    plt.show()

# Load the market layout
layout_file = "market_layout.json"
layout_data = load_market_layout(layout_file)
grid = convert_layout_to_grid(layout_data)

# Define cold aisles
cold_aisles = {'L', 'M', 'Q', 'T', 'U', 'W', 'X', 'Y'}

# Example input route with walking pattern
#example_route = "<->M->L->W->R->Z->C->U->G->F->V->P->S->A->D->T->B->Q->I->X->H->J->K->E->O->N->Y->>" #PW
example_route = "<->P->W->Q->E->T->A->H->F->Y->M->B->V->K->R->L->C->D->U->I->N->Z->O->X->G->S->J->>" #PB
#example_pattern = "ZgZg" #PW
example_pattern = "V2H" #PB
positions = find_positions(grid)
path_coords, time_steps, collected_items, collected_cold_items, foul_passes, decay_over_time = generate_walking_path(positions, example_route, example_pattern, cold_aisles)
print(len(foul_passes))
# Plot the movement analysis
plot_movement(time_steps, list(range(len(time_steps))), [d[1] for d in decay_over_time], collected_items, collected_cold_items, foul_passes)
