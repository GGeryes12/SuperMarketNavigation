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
            if grid[r, c] not in ['x', '0']:  # Exclude walls and empty spaces
                positions[grid[r, c]] = (r, c)
    return positions

def generate_walking_path(positions, route, walking_pattern):
    """Generates a step-by-step path following H2V, V2H, or ZgZg pattern."""
    path = route.split('->')
    path_coords = []
    visit_counts = {}
    
    if '<' in positions:
        path_coords.append(positions['<'])
        visit_counts[positions['<']] = 1
    
    for i in range(len(path) - 1):
        start = positions[path[i]]
        end = positions[path[i+1]]
        
        current_pos = start
        while current_pos != end:
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
            
            visit_counts[current_pos] = visit_counts.get(current_pos, 0) + 1
            path_coords.append(current_pos)
    
    if '>' in positions:
        path_coords.append(positions['>'])
        visit_counts[positions['>']] = visit_counts.get(positions['>'], 0) + 1
    
    return path_coords, visit_counts

def plot_walking_pattern(grid, route, walking_pattern):
    """Plots the walking path on the supermarket layout with different aisle types."""
    positions = find_positions(grid)
    cold_aisles = {'L', 'M', 'Q', 'T', 'U', 'W', 'X', 'Y'}
    entrance_exit = {'<': 'green', '>': 'red'}
    
    fig, ax = plt.subplots(figsize=(8, 8))
    ax.set_xticks(range(grid.shape[1]))
    ax.set_yticks(range(grid.shape[0]))
    ax.set_xticklabels([])
    ax.set_yticklabels([])
    ax.grid(True, linestyle='--', linewidth=0.5)
    
    # Draw walls
    for r in range(grid.shape[0]):
        for c in range(grid.shape[1]):
            if grid[r, c] == 'x':
                ax.add_patch(plt.Rectangle((c, r), 1, 1, color='black'))
    
    # Generate and plot walking path
    path_coords, visit_counts = generate_walking_path(positions, route, walking_pattern)
    
    # Plot aisles and entrance/exit
    for aisle, (r, c) in positions.items():
        color = 'gray' if aisle not in cold_aisles else 'blue'
        if aisle in entrance_exit:
            color = entrance_exit[aisle]
        ax.text(c + 0.5, r + 0.5, aisle, ha='center', va='center', fontsize=12, color='white', bbox=dict(facecolor=color, edgecolor='black'))
    
    # Plot path with directional arrows and different styles for repeated paths
    for i in range(len(path_coords) - 1):
        (r1, c1), (r2, c2) = path_coords[i], path_coords[i + 1]
        visits = visit_counts.get((r1, c1), 1)
        linestyle = '-' if visits == 1 else '--'
        ax.arrow(c1 + 0.5, r1 + 0.5, c2 - c1, r2 - r1, head_width=0.2, head_length=0.2, fc='red', ec='red', linestyle=linestyle)
    
    plt.gca().invert_yaxis()
    plt.title(f"Supermarket Walking Path ({walking_pattern})")
    plt.show()

# Load the market layout
layout_file = "market_layout.json"
layout_data = load_market_layout(layout_file)
grid = convert_layout_to_grid(layout_data)

# Example input route with walking pattern
example_route = "C->L->U->K->F->A->P->E->X->Q->B->R->I->D->V->M->N->J->T->W->H->Y->Z->S->G->O"
example_pattern = "ZgZg"
plot_walking_pattern(grid, example_route, example_pattern)