import { Profile } from "./profile";
import { ProfileTotalValue } from "./profileTotalValue";

export class ProfilePage {
    page: string;
    startTime: string;
    graphs: Profile[];
    periodTotals: ProfileTotalValue[];

    constructor() {
        this.graphs = [];
        this.periodTotals = [];
    }
}
