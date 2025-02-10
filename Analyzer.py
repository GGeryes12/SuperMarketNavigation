import pandas as pd
import numpy as np
import os
import matplotlib.pyplot as plt
from tqdm import tqdm
from scipy.spatial.distance import euclidean

# 🛠️ Function to find all CSV files in the directory structure
def find_all_csv_files(root_dir):
    """
    Traverses the directory structure to find CSV files inside each algorithm folder.
    Returns a list of (run, algorithm, file_path).
    """
    csv_files = []
    for run_folder in sorted(os.listdir(root_dir)):  # Iterate over Run1, Run2, etc.
        run_path = os.path.join(root_dir, run_folder)
        if os.path.isdir(run_path):
            for algo_folder in sorted(os.listdir(run_path)):  # Iterate over NSGA2Algorithm, NSGA3Algorithm, etc.
                algo_path = os.path.join(run_path, algo_folder)
                if os.path.isdir(algo_path):
                    for file in os.listdir(algo_path):  # Find the CSV file
                        if file.endswith('.csv'):
                            file_path = os.path.join(algo_path, file)
                            csv_files.append((run_folder, algo_folder, file_path))
    return csv_files

# 🛠️ Function to compute Pareto front
def pareto_front(df):
    df['WalkingTime'] = pd.to_numeric(df['WalkingTime'], errors='coerce')
    df['ExposureTime'] = pd.to_numeric(df['ExposureTime'], errors='coerce')
    df = df.dropna(subset=['WalkingTime', 'ExposureTime'])
    solutions = df[['WalkingTime', 'ExposureTime']].dropna().values
    solutions = np.array([list(map(float, sol)) for sol in solutions if len(sol) == 2])
    solutions = solutions[np.argsort(solutions[:, 0])]
    pareto_front = []
    best_exposure = float('inf')

    for sol in solutions:
        if sol[1] < best_exposure:
            pareto_front.append(sol.tolist())
            best_exposure = sol[1]

    return pd.DataFrame(pareto_front, columns=['WalkingTime', 'ExposureTime'])

# 🛠️ Function to compute Hypervolume (HV)
def compute_hypervolume(df, reference_point):
    pareto_solutions = df[['WalkingTime', 'ExposureTime']].values.tolist()
    pareto_solutions.sort(key=lambda x: x[0])
    hv = 0.0
    previous_point = reference_point[:]
    for point in pareto_solutions:
        width = abs(previous_point[0] - point[0])
        height = abs(reference_point[1] - point[1])
        hv += width * height
        previous_point = point
    return hv

# 🛠️ Function to compute IGD (Inverted Generational Distance)
def compute_igd(df, reference_pareto):
    obtained_solutions = df[['WalkingTime', 'ExposureTime']].values
    reference_solutions = reference_pareto[['WalkingTime', 'ExposureTime']].values

    if np.array_equal(obtained_solutions, reference_solutions):
        return 0.0  # Avoid unnecessary calculations

    igd_values = [np.min(np.linalg.norm(obtained_solutions - ref, axis=1)) for ref in reference_solutions]
    return np.mean(igd_values)

# 🛠️ Function to compute Spread
def compute_spread(df, pareto_df):
    pareto_points = pareto_df[['WalkingTime', 'ExposureTime']].values
    pareto_points = sorted(pareto_points, key=lambda x: x[0])
    distances = [euclidean(pareto_points[i], pareto_points[i+1]) for i in range(len(pareto_points)-1)]
    distances = np.array(distances)
    threshold = np.percentile(distances, 99)
    filtered_distances = distances[distances < threshold]
    if len(filtered_distances) == 0:
        return float('inf')
    mean_dist = np.mean(filtered_distances)
    return np.sum((filtered_distances - mean_dist) ** 2) / len(filtered_distances)

def compute_global_reference_point(root_dir):
    """
    Computes a single reference point across all runs and algorithms.
    """
    all_max_walking = []
    all_max_exposure = []

    for run, algorithm, file_path in find_all_csv_files(root_dir):
        df = pd.read_csv(file_path)
        all_max_walking.append(df['WalkingTime'].max())
        all_max_exposure.append(df['ExposureTime'].max())

    return [max(all_max_walking), max(all_max_exposure)]

def compute_reference_pareto(pareto_df):
    """
    Computes a unified reference Pareto front across all runs and algorithms.
    """
    all_solutions = pareto_df[['WalkingTime', 'ExposureTime']].values.tolist()

    # Remove dominated solutions to get the best known Pareto front
    reference_solutions = []
    for sol in all_solutions:
        dominated = False
        for other in all_solutions:
            if (other[0] <= sol[0] and other[1] <= sol[1]) and (other[0] < sol[0] or other[1] < sol[1]):
                dominated = True
                break
        if not dominated:
            reference_solutions.append(sol)

    return pd.DataFrame(reference_solutions, columns=['WalkingTime', 'ExposureTime'])


def analyze_runs(root_dir):
    """
    Processes all CSV files in the directory structure.
    Computes performance metrics & Pareto solutions.
    Stores all final-generation Pareto solutions.
    """
    csv_files = find_all_csv_files(root_dir)
    print(f"🔍 Found {len(csv_files)} CSV files. Starting analysis...\n")

    # Compute a single reference point for HV
    global_reference_point = compute_global_reference_point(root_dir)
    print(f"✅ Using Global Reference Point for HV: {global_reference_point}\n")

    performance_results = []
    pareto_results = []
    all_final_pareto = []  # Stores all last-generation Pareto solutions

    # **Step 1: First pass - Extract Pareto fronts for all runs**
    with tqdm(total=len(csv_files), desc="Extracting Pareto Fronts", unit="file") as pbar:
        for run, algorithm, file_path in csv_files:
            df = pd.read_csv(file_path)
            pareto_df = pareto_front(df)

            # Store Pareto solutions per run & algorithm
            pareto_df['Run'] = run
            pareto_df['Algorithm'] = algorithm
            pareto_results.append(pareto_df)

            # **Store final Pareto solutions in a global table**
            for _, row in pareto_df.iterrows():
                all_final_pareto.append({
                    'Algorithm': algorithm,
                    'Run': run,
                    'WalkingTime': row['WalkingTime'],
                    'ExposureTime': row['ExposureTime']
                })

            pbar.update(1)

    # Convert Pareto results to DataFrame
    pareto_df = pd.concat(pareto_results, ignore_index=True)

    # **Step 2: Compute Unified Reference Pareto Front**
    print("\n🔍 Computing Unified Reference Pareto Front...")
    reference_pareto = compute_reference_pareto(pareto_df)
    print(f"✅ Reference Pareto Front computed with {len(reference_pareto)} solutions.\n")

    # **Step 3: Second pass - Compute IGD and other metrics**
    with tqdm(total=len(csv_files), desc="Computing Metrics", unit="file") as pbar:
        for run, algorithm, file_path in csv_files:
            df = pd.read_csv(file_path)
            pareto_df = pareto_front(df)

            hv = compute_hypervolume(pareto_df, global_reference_point)  # Use global reference
            igd = compute_igd(pareto_df, reference_pareto)  # Use the computed reference
            spread = compute_spread(df, pareto_df)

            # Store performance metrics per run & algorithm
            performance_results.append({
                'Run': run,
                'Algorithm': algorithm,
                'Hypervolume': hv,
                'IGD': igd,
                'Spread': spread,
                'Pareto Solutions': len(pareto_df)
            })

            pbar.update(1)

    print("\n✅ Analysis Completed!")

    # Convert lists to DataFrames
    performance_df = pd.DataFrame(performance_results)
    all_final_pareto_df = pd.DataFrame(all_final_pareto)  # Store all Pareto solutions

    return performance_df, pareto_df, all_final_pareto_df


# **Main Execution**
if __name__ == "__main__":
    root_directory = r"C:\Users\Jeryes\github\SuperMarketNavigation\MultiObject\SuperMarketNavigation\bin\Debug\net9.0\Run_20250209_004552"
    
    performance_df, pareto_df, all_final_pareto_df = analyze_runs(root_directory)


    # Save results
    performance_df.to_csv("performance_results.csv", index=False)
    pareto_df.to_csv("pareto_results.csv", index=False)
    all_final_pareto_df.to_csv("all_final_pareto.csv", index=False)

    # Display summary
    print("\n📊 **Performance Metrics Table:**")
    print(performance_df)

    print("\n📊 **Best Pareto Solutions Table:**")
    print(pareto_df.head())
