import { IStatsState } from "@/interfaces/IStats";
import { IAction } from "@/interfaces/IAction";
import { ADD_STAT, RESET_STATS, ADD_STAT_POINT, REMOVE_STAT_POINT } from "./stats.types";

const initialState: IStatsState = {
  selectedStats: [],
  gradedStats: [],
  statPointsAvailable: 30,
  totalStatPoints: 30,
};

const reducer = (state: IStatsState = initialState, { type, payload }: IAction) => {
  switch (type) {
    case ADD_STAT:
      const { selectedStats, gradedStats, statPointsAvailable } = state;

      let newStats = [...selectedStats];
      let newGradedStats = [...gradedStats];
      let pointsCollected = 0;

      /* remove if adding same stat again */
      const index = selectedStats.findIndex((stat) => !!stat && stat.name === payload.stat.name);
      if (index !== -1) {
        /* stats should be in sync with graded */
        newStats.splice(index, 1);

        /* just making sure we don't remove some other stat */
        const gradedIndex = selectedStats.findIndex((stat) => !!stat && stat.name === payload.stat.name);
        /* take assigned points in store */
        pointsCollected += newGradedStats[gradedIndex].level;
        newGradedStats.splice(index, 1);
      } else if (selectedStats.length < 4) {
        newStats = [...selectedStats, payload.stat];
        const points = 0;
        const newStat = {
          ...payload.stat,
          level: points,
        };
        newGradedStats = [...gradedStats, newStat];
      }

      return {
        ...state,
        selectedStats: newStats,
        gradedStats: newGradedStats,
        statPointsAvailable: statPointsAvailable + pointsCollected,
      };

    case ADD_STAT_POINT: {
      const { stat } = payload;
      const { gradedStats, statPointsAvailable } = state;

      if (statPointsAvailable <= 0) {
        return {
          ...state,
        };
      } else {
        const newGradedStats = gradedStats.map((gradedStat) => {
          let { level } = gradedStat;
          if (gradedStat.name === stat.name) {
            level = level + 1;
          }
          return {
            ...gradedStat,
            level,
          };
        });

        return {
          ...state,
          gradedStats: newGradedStats,
          statPointsAvailable: statPointsAvailable - 1,
        };
      }
    }

    case REMOVE_STAT_POINT: {
      const { stat } = payload;
      const { gradedStats, statPointsAvailable } = state;

      if (statPointsAvailable >= 40 || stat.level === 0) {
        return {
          ...state,
        };
      } else {
        const newGradedStats = gradedStats.map((gradedStat) => {
          let { level } = gradedStat;
          if (gradedStat.name === stat.name) {
            level = level - 1;
          }
          return {
            ...gradedStat,
            level,
          };
        });

        return {
          ...state,
          gradedStats: newGradedStats,
          statPointsAvailable: statPointsAvailable + 1,
        };
      }
    }

    case RESET_STATS:
      return { ...initialState };

    default:
      return state;
  }
};

export default reducer;
