#!/bin/bash
#SBATCH --nodes=1
#SBATCH --gpus-per-node=1
#SBATCH --time=0:10:00
#SBATCH --mem=3000

module purge
apptainer pull docker://sarahema/castles:command

apptainer exec --nv castles_command.sif mlagents-learn /home1/s3747328/castles/multitraining.yaml --run-id=Habrok7 --env=/home1/s3747328/castles/castles_build_1 --no-graphics 2>&1 | tee output
