# Cities
A procgen city project.

**Insights**

This project is primarily an experiment in data structures. The primary goal was to explore the means of recursive subdivisions to represent the city structure. For example, a city perimeter gets subdivided into regions by the main through roads. These regions get filled out with city blocks, parks, and shopping areas. 

Observing real cities, you can tell a lot about what to expect on a given street by knowing what neighborhood you're in. As such, I planned the recursive structure to have the ability to represent qualities like neighborhood wealth, districting, etc. With these high level settings, the subdivided streets could be generated differently. 

The project hasn't made it to that level of depth, but still, it represents a successful experiment in thinking in terms of flexible data structures. 

**Data Structures**

Linked Graph is the core structure. This works in some ways like a multi-linked list. The structure has a spatial element in that each edge forming the graph has vertices with a position. 

Built on top of the Linked Graph, is a concept of edge loops. Edge loops represent loops of edges formed in the graph. These loops become the super-class of other classes such as SubdividableEdgeLoop, and CityRegion. 

The core generation loop happens in stages within the City class. At each stage, the city is recursively subdivided until all subdivision at a certain stage is complete. The stages are: 0: primary city structure and blocks, 1: plots (parks and non-parks), and 2: the final building footprints, with space carved out for roads. 

Different city regions have implement subdivision differently, allowing for flexible changes to the city layout and structure. 

While I believe in this structure for the most part, it certainly has limitation. In some cases, this codebase can also feel a little overengineered. Be warned. 

**AI**

I have recently endeavored to add some AI features to this project. My idea was to allow users to walk around the city streets and meet AI avatars in the houses. These avatars would have simulated conversations and interactions with eachother. By walking around, you can learn about the social networks and gossip of the city. This feature is now working at a MVP level. 

The AI folder contains code for calling GPT-5-nano, parsing results, and storing them in a data structure called TownSimulation. The TownSimulation class has functionality to allow for two of its residents to encounter, and for an overview of their encounter to be added to their respective knowledge bases. Additionally, this class supports save/load from json and batch simulation. 