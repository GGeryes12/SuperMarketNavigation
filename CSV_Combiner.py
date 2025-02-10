import pandas as pd
import numpy as np
import os
from tqdm import tqdm

def find_all_csv_files(root_dir):
    """Traverses the directory structure to find CSV files inside each algorithm folder."""
    csv_files = []
    for run_folder in sorted(os.listdir(root_dir)):
        run_path = os.path.join(root_dir, run_folder)
        if os.path.isdir(run_path):
            for algo_folder in sorted(os.listdir(run_path)):
                algo_path = os.path.join(run_path, algo_folder)
                if os.path.isdir(algo_path):
                    for file in os.listdir(algo_path):
                        if file.endswith('.csv'):
                            file_path = os.path.join(algo_path, file)
                            csv_files.append((run_folder, algo_folder, file_path))
    return csv_files

def merge_all_runs(root_dir, output_file="merged_raw_data.csv"):
    """Merges all raw data solutions from all runs and algorithms into a unified CSV file."""
    csv_files = find_all_csv_files(root_dir)
    print(f"🔍 Found {len(csv_files)} CSV files. Starting merging process...\n")
    
    all_data = []
    with tqdm(total=len(csv_files), desc="Processing Files", unit="file") as pbar:
        for run, algorithm, file_path in csv_files:
            df = pd.read_csv(file_path)
            df['Run'] = run
            df['Algorithm'] = algorithm
            all_data.append(df)
            pbar.update(1)
    
    merged_df = pd.concat(all_data, ignore_index=True)
    merged_df.to_csv(output_file, index=False)
    print(f"✅ Merging completed! Data saved to {output_file}\n")
    return merged_df

# **Main Execution**
if __name__ == "__main__":
    root_directory = r"C:\Users\Jeryes\github\SuperMarketNavigation\MultiObject\SuperMarketNavigation\bin\Debug\net9.0\Run_20250209_004552"
    merged_df = merge_all_runs(root_directory)
    print("\n📊 **Merged Data Sample:**")
    print(merged_df.head())
