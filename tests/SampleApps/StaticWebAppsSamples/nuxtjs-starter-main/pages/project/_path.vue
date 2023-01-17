<template>
  <div class="project">
    <aside>
      <h3>You can deploy...</h3>
      <ul>
        <li v-for="project in projects" v-bind:key="project.id">
          <a v-bind:href="`/project/${project.slug}`">{{ project.name }}</a>
        </li>

        <li>
          <a href="/">Home</a>
        </li>
      </ul>
    </aside>
    <main>
      <div class="card-big">
        <component w="249" h="278" v-bind:is="projectData.id"></component>
        <!-- <Icon w="{249}" h="{278}" /> -->
        <div class="stats">
          <div class="stats-details">
            <div>
              <StarIcon v-bind:w="18" v-bind:h="18" />
              <p>{{ project.stargazers_count }}</p>
            </div>
            <p>stars</p>
          </div>
          <div class="stats-details">
            <div>
              <WatchIcon v-bind:w="18" v-bind:h="18" />
              <p>{{ project.subscribers_count }}</p>
            </div>
            <p>watchers</p>
          </div>
          <div class="stats-details">
            <div>
              <BugIcon v-bind:w="18" v-bind:h="18" />
              <p>{{ project.open_issues }}</p>
            </div>
            <p>issues</p>
          </div>
        </div>
        <p class="description">{{ project.description }}</p>
        <div class="cta">
          <a
            class="button-github"
            v-bind:href="project.html_url"
            target="_blank"
          >
            <GithubIcon v-bind:w="24" v-bind:h="24" />
            Learn more...
          </a>
        </div>
      </div>
    </main>
  </div>
</template>

<script>
import fetch from "isomorphic-unfetch";
import projectIcons, {
  StarIcon,
  WatchIcon,
  BugIcon,
  AzureIcon,
  GithubIcon
} from "../../components/Icons.vue";
import { projects } from "../../utils/projectsData";
export default {
  data() {
    return {
      projects,
      path: this.$route.params.path,
      projectData: projects.find(
        project => project.slug === this.$route.params.path
      )
    };
  },
  computed: {
    href: function() {
      return `/project/${this.projectData.slug}`;
    }
  },
  components: {
    ...projectIcons,
    StarIcon,
    WatchIcon,
    BugIcon,
    AzureIcon,
    GithubIcon
  },
  async asyncData({ params, $axios, payload }) {
    let project;
    const { path } = params;
    const projectData = projects.find(project => project.slug === path);
    const ghPath = projectData.path;
    if (payload) {
      project = payload;
      console.log("getting post", project.id, "from payload");
    } else {
      const res = await fetch(`https://api.github.com/repos/${ghPath}`);
      project = await res.json();
      console.log("hitting the API for the post", project.id);
    }
    return { project };
  }
};
</script>
