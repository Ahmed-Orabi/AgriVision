from setuptools import find_packages, setup
from typing import List

hypen_e_dot = '-e .'
def get_requirements(file_path:str)->List[str]:
    '''
    this function will return all requirements
    '''
    req = []
    with open(file_path,'r') as file_obj:
        req = file_obj.readlines()
        req = [r.replace('\n', '') for r in req]

        if hypen_e_dot in req:
            req.remove(hypen_e_dot)
        
        return req

setup(
    name='Fertilizer Recommendation System',
    version='0.01',
    author='Ahmed AlaaEldin',
    author_email='ahmeddaladdin@gmail.com',
    packages=find_packages(),
    install_requires=get_requirements('requirements.txt')
)