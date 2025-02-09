# -*- coding: utf-8 -*-
"""
Created on Sun Feb  9 01:16:00 2025

@author: Jeryes
"""

import pandas as pd
import numpy as np
from pygmo import hypervolume
from scipy.spatial.distance import euclidean

# Load CSV file
def load_data(file_path):
    df = pd.read_csv(file_path)
    return df

# Compute Non-Dominated Solutions (Pareto Front)
def pareto_front(df):
    solutions = df[['WalkingTime', 'ExposureTime']].values
    non_dominated = []
    
    for i, sol in enumerate(solutions):
        dominated = False
        for j, other in enumerate(solutions):
            if all(other <= sol) and any(other < sol):  # Other solution dominates
                dominated = True
                break
        if not dominated:
            non_dominated.append(tuple(sol))
    
    pareto_df = df[df[['WalkingTime', 'ExposureTime']].apply(tuple, axis=1).isin(non_dominated)]
    return pareto_df

# Compute Hypervolume (HV)
def compute_hypervolume(df, reference_point):
    points = df[['WalkingTime', 'ExposureTime']].values.tolist()
    hv = hypervolume(points)
    return hv.compute(reference_point)

# Compute Spread Metric
def compute_spread(df, pareto_df):
    pareto_points = pareto_df[['WalkingTime', 'ExposureTime']].values
    pareto_points = sorted(pareto_points, key=lambda x: x[0])  # Sort by WalkingTime
    
    # Compute pairwise Euclidean distances
    distances = [euclidean(pareto_points[i], pareto_points[i+1]) for i in range(len(pareto_points)-1)]
    mean_dist = np.mean(distances)
    
    # Compute spread metric
    spread = np.sum((distances - mean_dist) ** 2) / len(distances)
    return spread

# Compute Inverted Generational Distance (IGD)
def compute_igd(df, reference_pareto):
    obtained_solutions = df[['WalkingTime', 'ExposureTime']].values
    reference_solutions = reference_pareto[['WalkingTime', 'ExposureTime']].values
    
    total_distance = 0
    for ref in reference_solutions:
        min_dist = min(euclidean(ref, sol) for sol in obtained_solutions)
        total_distance += min_dist
    
    return total_distance / len(reference_solutions)

# Main function
def main(file_path):
    df = load_data(file_path)
    pareto_df = pareto_front(df)

    reference_point = [df['WalkingTime'].max(), df['ExposureTime'].max()]
    hv = compute_hypervolume(pareto_df, reference_point)
    spread = compute_spread(df, pareto_df)
    igd = compute_igd(df, pareto_df)

    print(f"Hypervolume (HV): {hv}")
    print(f"Spread: {spread}")
    print(f"Inverted Generational Distance (IGD): {igd}")
    print("\nBest Non-Dominated Solutions (Pareto Front):")
    print(pareto_df)

# Example usage
if __name__ == "__main__":
    file_path = r"C:\Users\Jeryes\github\SuperMarketNavigation\MultiObject\SuperMarketNavigation\bin\Debug\net9.0\Run_20250209_004552\Run1\NSGA2Algorithm\NSGA2Algorithm_raw_data.csv"  # Replace with actual CSV path
    main(file_path)
