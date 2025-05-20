As per usual, everything in this devlog is subject to change and things may end up working out differently than expected!

----
### Devlog 027

In shapez 2, the core gameplay is quite simple: The player places a building and then it starts working. However, quite some effort is necessary to make all the buildings that are placed work together as the player expects.

Besides the pure functionality of a factory, performance is a key element for shapez 2. The goal has always been to enable the player to build massive factories, in an enjoyable way. From a technical point of view, this means that shapez 2 must be capable of simulation millions of buildings while maintaining a stable framerate and must react fluently when the players make changes to their factories.

We will first have a look at the state of development when the shapez 2 Demo was released, then the state of development of the current shapez 2 Early Access version, and finally the improvements with the upcoming 0.0.9 and 0.1.0 release. Let’s go!

----
### Past

During the development of the shapez 2 Demo, we focused on precise functionality and a sustainable way to create new buildings quickly. This time was not about scale, but to implement a solid basis which we can easily improve later.

----
**Basics**

All buildings are assembled from common components like belts, pipes, and fluid containers. In addition, most buildings also have some custom simulation logic.

For example, the Rotator consists of three successive belt lanes. The input lane on the left and the output lane on the right are visualized as conveyors and will just move shapes forward. The processing lane at the center is visualized as a circle. It has a length of zero and does not move the shape. Instead, it contains the custom simulation logic that performs the rotation of the shape.

![[Pasted image 20250518140248.png]]**Connections**

Each building also defines input and output connectors. For example, the entry to the Rotator’s first belt lane is defined as its input connector, and the exit of its last belt lane is defined as its output connector.

[![r/shapezio - Devlog 027 - Improving the Simulation](https://preview.redd.it/devlog-027-improving-the-simulation-v0-0a41d8c3sn3e1.png?width=315&format=png&auto=webp&s=8045aba7e1c55f02b039475319415d5ac19fcd31)](https://preview.redd.it/devlog-027-improving-the-simulation-v0-0a41d8c3sn3e1.png?width=315&format=png&auto=webp&s=8045aba7e1c55f02b039475319415d5ac19fcd31 "Image from r/shapezio - Devlog 027 - Improving the Simulation")

Whenever a building is placed, we check if adjacent buildings have compatible connectors at the same edge. If we find a compatible connector, we connect the two buildings.

[![r/shapezio - Devlog 027 - Improving the Simulation](https://preview.redd.it/devlog-027-improving-the-simulation-v0-e56rsm24sn3e1.png?width=323&format=png&auto=webp&s=447adbb368f222b0dcdf360906e9fe92a2539fa0)](https://preview.redd.it/devlog-027-improving-the-simulation-v0-e56rsm24sn3e1.png?width=323&format=png&auto=webp&s=447adbb368f222b0dcdf360906e9fe92a2539fa0 "Image from r/shapezio - Devlog 027 - Improving the Simulation")

In the example, the incoming Conveyor on the left will hand over shapes to the Rotator, as soon as they reach the end of the conveyor’s belt lane. The Rotator will hand over shapes to the outgoing Conveyor when they reach the end of the Rotator’s output belt lane.

----

**Update Order**

When the simulation is updated, it will move forward all shapes on any belt lanes in the factory. To move a shape forward, there must be enough space in front. However, on a full belt, there is no logical space between the shapes, even though we render the shapes with a gap for visual clarity.

[![r/shapezio - Devlog 027 - Improving the Simulation](https://preview.redd.it/devlog-027-improving-the-simulation-v0-8v9uw9t4sn3e1.png?width=316&format=png&auto=webp&s=20fd6845fb90a732ec30f6b390605f0c3983e5ea)](https://preview.redd.it/devlog-027-improving-the-simulation-v0-8v9uw9t4sn3e1.png?width=316&format=png&auto=webp&s=20fd6845fb90a732ec30f6b390605f0c3983e5ea "Image from r/shapezio - Devlog 027 - Improving the Simulation")

In the example, you see five shapes and their respective, logical size. None of the shapes could be moved forward because there no space in front of them.

To be able to move the shapes forward anyway, we must update the buildings and their components in the right order. Therefore we start updating at the end and finish updating at the start of a shape’s path. This usually means that we move the shapes at the entrance to the Vortex first and the shapes at the Extractors last.

For instance, the Conveyor at Rotator’s output is updated before the Rotator. The Rotator then updates its output, processing, and input lanes - in this order. Finally, the Conveyor at the Rotator’s input is updated after the Rotator.

[![r/shapezio - Devlog 027 - Improving the Simulation](https://preview.redd.it/devlog-027-improving-the-simulation-v0-f6dvsdf5sn3e1.png?width=460&format=png&auto=webp&s=f71e5f76c52482c1b4f9c22d6e3db3368d17474e)](https://preview.redd.it/devlog-027-improving-the-simulation-v0-f6dvsdf5sn3e1.png?width=460&format=png&auto=webp&s=f71e5f76c52482c1b4f9c22d6e3db3368d17474e "Image from r/shapezio - Devlog 027 - Improving the Simulation")

With these requirements, we can compute an optimal update order for any setup of buildings.

[![r/shapezio - Devlog 027 - Improving the Simulation](https://preview.redd.it/devlog-027-improving-the-simulation-v0-ilfb5hx5sn3e1.png?width=789&format=png&auto=webp&s=dbbfd1987570b35712b3e7d61c5caa5bbc821ee3)](https://preview.redd.it/devlog-027-improving-the-simulation-v0-ilfb5hx5sn3e1.png?width=789&format=png&auto=webp&s=dbbfd1987570b35712b3e7d61c5caa5bbc821ee3 "Image from r/shapezio - Devlog 027 - Improving the Simulation")

**If you're interested in learning more about our early optimization efforts, be sure to check out Devlog 011!**
[https://steamcommunity.com/games/2162800/announcements/detail/3710460746137286715](https://steamcommunity.com/games/2162800/announcements/detail/3710460746137286715)

----
# Present

During the development of the shapes 2 Early Access version we focused on scale. We had to improve the performance of a running factory and improve the performance when players make changes to their factory. We also had to find solutions for some special requirements.

At the release of the shapez 2 Demo, controlling the Belt Launchers and Belt Catchers was one of the biggest issues. Unlike the other buildings, they had to behave differently depending on the 'constellation' – the relative positioning of the buildings. Binding the custom simulation logic directly to a building did not work out well in these cases.

**Pattern Matching**

For the Early Access, we introduced pattern matching to decide which simulation logic is applied to each building. This means, that whenever a building is placed, we can first check the building’s surroundings and then decide on one of multiple available custom simulation logic.

We can also aggregate multiple buildings into a single simulation logic. This grants precise control of building behaviors in different constellations and opportunities for performance optimizations.

As an example, a Belt Launcher without a corresponding Belt Catcher results in a simulation logic that blocks incoming shapes. A Belt catcher without a corresponding Belt Launcher results in a simulation logic that doesn’t do anything. Only a Belt Launcher with a matching Belt Catcher results in a single simulation logic for both buildings together, capable of throwing shapes from one building to the other.

[![r/shapezio - Devlog 027 - Improving the Simulation](https://preview.redd.it/devlog-027-improving-the-simulation-v0-v6e3otz7sn3e1.png?width=833&format=png&auto=webp&s=4ca174f4787d207103da86631333ba70bb409e63)](https://preview.redd.it/devlog-027-improving-the-simulation-v0-v6e3otz7sn3e1.png?width=833&format=png&auto=webp&s=4ca174f4787d207103da86631333ba70bb409e63 "Image from r/shapezio - Devlog 027 - Improving the Simulation")

We can also make use of pattern matching to improve performance. For example, we now aggregate all directly connected Conveyors into a single simulation logic. This reduces the required computations dramatically.

[![r/shapezio - Devlog 027 - Improving the Simulation](https://preview.redd.it/devlog-027-improving-the-simulation-v0-ci3we3l8sn3e1.png?width=850&format=png&auto=webp&s=01821943b264ed1f68bf3b4b6c2a8eba2ffab1e6)](https://preview.redd.it/devlog-027-improving-the-simulation-v0-ci3we3l8sn3e1.png?width=850&format=png&auto=webp&s=01821943b264ed1f68bf3b4b6c2a8eba2ffab1e6 "Image from r/shapezio - Devlog 027 - Improving the Simulation")

----
**Update Graph**

Whenever buildings are added or removed, the update order has to be recomputed. This is a very expensive process. So we searched for a way to reduce this effort.

For Early Access, we no longer compute the update order directly. Instead, we maintain a directed graph of simulation logic that determines the update order. This is much faster, as we usually only need to attach or detach a single node to the graph. More expensive computations are necessary only when the player makes big changes to the graph, like placing a big blueprint.

[![r/shapezio - Devlog 027 - Improving the Simulation](https://preview.redd.it/devlog-027-improving-the-simulation-v0-ffjye6l9sn3e1.png?width=752&format=png&auto=webp&s=3d6ab36d0106a19f66d83d21e6e896c7d7d0b4f3)](https://preview.redd.it/devlog-027-improving-the-simulation-v0-ffjye6l9sn3e1.png?width=752&format=png&auto=webp&s=3d6ab36d0106a19f66d83d21e6e896c7d7d0b4f3 "Image from r/shapezio - Devlog 027 - Improving the Simulation")

In the example, you can see how the connections between the simulation logic of of all placed buildings create a graph.

----

**Clusters**

Another benefit of a graph is that we can identify isolated subgraphs. These are parts of the graph that are not connected to other parts of the graph.

[![r/shapezio - Devlog 027 - Improving the Simulation](https://preview.redd.it/devlog-027-improving-the-simulation-v0-bo8j377asn3e1.png?width=750&format=png&auto=webp&s=704939d03aa632b2fa0385b34f529e52fb8031ba)](https://preview.redd.it/devlog-027-improving-the-simulation-v0-bo8j377asn3e1.png?width=750&format=png&auto=webp&s=704939d03aa632b2fa0385b34f529e52fb8031ba "Image from r/shapezio - Devlog 027 - Improving the Simulation")

In the example, you can see that the Update Graph consists of three isolated subgraphs (blue, red, and green). To visualize a subgraph while playing, select a building and press the “Select Connected” hotkey “O”.

For shapez 2, we move these subgraphs into structures we call clusters. We can benefit from these Clusters in multiple ways.

First, even if we need to recompute an Update Order completely, we only need to do it for one cluster ignoring all other clusters. This especially improves the flow of the game when you place bigger blueprints. We can now also define an individual update behavior for each single cluster.

----
**Update Frequency**

Before introducing the clusters, we updated each simulation logic in every frame.

[![r/shapezio - Devlog 027 - Improving the Simulation](https://preview.redd.it/devlog-027-improving-the-simulation-v0-oz1dshebsn3e1.png?width=3591&format=png&auto=webp&s=ba448e8e5862ac324f26499429bf03200108ebbd)](https://preview.redd.it/devlog-027-improving-the-simulation-v0-oz1dshebsn3e1.png?width=3591&format=png&auto=webp&s=ba448e8e5862ac324f26499429bf03200108ebbd "Image from r/shapezio - Devlog 027 - Improving the Simulation")

With the introduction of clusters, we don’t do this anymore. Clusters that are far away receive an update only every few frames. And clusters that are out of view are updated only three times per second.

[![r/shapezio - Devlog 027 - Improving the Simulation](https://preview.redd.it/devlog-027-improving-the-simulation-v0-dtut4q3csn3e1.png?width=1935&format=png&auto=webp&s=08744da7b04589945efa8bb5120254e973dffa13)](https://preview.redd.it/devlog-027-improving-the-simulation-v0-dtut4q3csn3e1.png?width=1935&format=png&auto=webp&s=08744da7b04589945efa8bb5120254e973dffa13 "Image from r/shapezio - Devlog 027 - Improving the Simulation")

During the endgame of shapez 2, players currently easily place up to 500.000 buildings in their factories, resulting in several thousand clusters. Only the handful of buildings in view must be updated every frame, allowing the Early Access version to support about 20 times bigger factories than in the Demo version.

----
# Future

Heading towards shapez 2 Update 1, we will further improve the performance of the game to finally support truly massive factories. The 0.0.9 release – set to come very soon – will give you a foretaste of the future scale of shapez 2. Here are some of the things included in the update:

In the general settings menu, you will find two additional settings in the simulation section that enable new performance features.

[![r/shapezio - Devlog 027 - Improving the Simulation](https://preview.redd.it/devlog-027-improving-the-simulation-v0-fntc19ggsn3e1.png?width=481&format=png&auto=webp&s=d9694a5b15dfea51c476e93f02c161af1cc908d6)](https://preview.redd.it/devlog-027-improving-the-simulation-v0-fntc19ggsn3e1.png?width=481&format=png&auto=webp&s=d9694a5b15dfea51c476e93f02c161af1cc908d6 "Image from r/shapezio - Devlog 027 - Improving the Simulation")

**Simulation Threads**

Clusters don’t interfere with other clusters during their update. This means we can make use of all available CPU cores and update multiple clusters at the same time.

[![r/shapezio - Devlog 027 - Improving the Simulation](https://preview.redd.it/devlog-027-improving-the-simulation-v0-cw7ogsxgsn3e1.png?width=1595&format=png&auto=webp&s=b9865d2e84de8a5b3a89b49e9d68d6f0c7defe89)](https://preview.redd.it/devlog-027-improving-the-simulation-v0-cw7ogsxgsn3e1.png?width=1595&format=png&auto=webp&s=b9865d2e84de8a5b3a89b49e9d68d6f0c7defe89 "Image from r/shapezio - Devlog 027 - Improving the Simulation")

In the example above, you can see how using a second core already improves the simulation performance by 100%. Depending on your hardware and the number of other processes running on your computer while playing shapez 2, this can speed up the simulation update by up to 2000%. AMD CPUs especially should see big improvements, as they tend to have a lot of cores. Again, [join our Discord](https://discord.gg/bvq5uGxW8G) if you'd like to try this update earlier ;)

**Parallel Rendering**

During the game’s update loop, we need to do several things. By far the most expensive are the simulation update and the rendering.

[![r/shapezio - Devlog 027 - Improving the Simulation](https://preview.redd.it/devlog-027-improving-the-simulation-v0-zvt7o7lhsn3e1.png?width=3591&format=png&auto=webp&s=894f770c8d4f6cbb12fe93f96ba12bf1ca539fad)](https://preview.redd.it/devlog-027-improving-the-simulation-v0-zvt7o7lhsn3e1.png?width=3591&format=png&auto=webp&s=894f770c8d4f6cbb12fe93f96ba12bf1ca539fad "Image from r/shapezio - Devlog 027 - Improving the Simulation")

Already during the development of the shapez 2 Demo, we decoupled the actual rendering from the gathering of all the information for the rendering. Therefore, the rendering is already decoupled from the simulation update. This made it a relatively small step to do the simulation update in parallel to the rendering. Depending on your hardware setup, this may double your performance as well.
[![r/shapezio - Devlog 027 - Improving the Simulation](https://preview.redd.it/devlog-027-improving-the-simulation-v0-e6dboa7isn3e1.png?width=3027&format=png&auto=webp&s=a8615f4f9331d672e6c355d4e9cca6c8566a2939)](https://preview.redd.it/devlog-027-improving-the-simulation-v0-e6dboa7isn3e1.png?width=3027&format=png&auto=webp&s=a8615f4f9331d672e6c355d4e9cca6c8566a2939 "Image from r/shapezio - Devlog 027 - Improving the Simulation")

---

We hope you enjoyed this devlog and maybe even learned something new! Soon, some of these changes will go live and hopefully give you a significant boost in performance. See you then!

*~ Nicko & the shapez 2 Team*