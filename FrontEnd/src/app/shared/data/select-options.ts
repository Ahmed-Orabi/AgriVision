export type SelectOption = string | { label: string; value: string };

export const FARM_OPTIONS = ['Farm 1 - North', 'Farm 2 - East', 'Farm 3 - South', 'Farm 4 - West'];

export const SOIL_OPTIONS = ['Loamy Soil', 'Peaty Soil', 'Acidic Soil', 'Neutral Soil', 'Alkaline Soil'];

export const FARM_UNIT_OPTIONS: SelectOption[] = [
  { label: 'Meters (m)', value: 'm' },
  { label: 'Kilometers (km)', value: 'km' },
];

export const COUNTRY_OPTIONS = ['Egypt', 'Saudi Arabia', 'United Arab Emirates', 'United States', 'United Kingdom'];

export const CROP_OPTIONS = [
  'rice',
  'wheat',
  'Mung Bean',
  'Tea',
  'millet',
  'maize',
  'Lentil',
  'Jute',
  'Coffee',
  'Cotton',
  'Ground Nut',
  'Peas',
  'Rubber',
  'Sugarcane',
  'Tobacco',
  'Kidney Beans',
  'Moth Beans',
  'Coconut',
  'Black gram',
  'Adzuki Beans',
  'Pigeon Peas',
  'Chickpea',
  'banana',
  'grapes',
  'apple',
  'mango',
  'muskmelon',
  'orange',
  'papaya',
  'pomegranate',
  'watermelon',
  'potatoes',
  'tomatoes',
  'berseem clover',
];

export const IRRIGATION_SOIL_OPTIONS = ['Sandy', 'Loamy', 'Clay', 'Silty', 'Peaty', 'Chalky'];

export const IRRIGATION_CROP_OPTIONS = ['Wheat', 'Rice', 'Maize', 'Cotton', 'Tomato', 'Potato', 'Sugarcane', 'Citrus'];

export const GROWTH_STAGE_OPTIONS = ['Seedling', 'Vegetative', 'Flowering', 'Fruiting', 'Maturity'];

export const SEASON_OPTIONS = ['Winter', 'Spring', 'Summer', 'Autumn'];
